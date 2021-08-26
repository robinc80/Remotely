using Remotely.Agent.Interfaces;
using Remotely.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Remotely.Agent.Services
{
    public class UpdaterMac : IUpdater
    {
        private readonly string _achitecture = RuntimeInformation.OSArchitecture.ToString().ToLower();
        private readonly SemaphoreSlim _checkForUpdatesLock = new SemaphoreSlim(1, 1);
        private readonly ConfigService _configService;
        private readonly IWebClientEx _webClientEx;
        private readonly SemaphoreSlim _installLatestVersionLock = new SemaphoreSlim(1, 1);
        private DateTimeOffset _lastUpdateFailure;
        private readonly System.Timers.Timer _updateTimer = new System.Timers.Timer(TimeSpan.FromHours(6).TotalMilliseconds);

        public UpdaterMac(ConfigService configService, IWebClientEx webClientEx)
        {
            _configService = configService;
            _webClientEx = webClientEx;
            _webClientEx.SetRequestTimeout((int)_updateTimer.Interval);
        }


        public async Task BeginChecking()
        {
            if (EnvironmentHelper.IsDebug)
            {
                return;
            }

            await CheckForUpdates();
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.Start();
        }

        public async Task CheckForUpdates()
        {
            try
            {
                await _checkForUpdatesLock.WaitAsync();

                if (EnvironmentHelper.IsDebug)
                {
                    return;
                }

                if (_lastUpdateFailure.AddDays(1) > DateTimeOffset.Now)
                {
                    Logger.Write("Vérification ignorée suite à des tentatives infructueuses.  Redémarrez le serveur ou lancez l'installation manuellement.");
                    return;
                }


                var connectionInfo = _configService.GetConnectionInfo();
                var serverUrl = _configService.GetConnectionInfo().Host;

                var fileUrl = serverUrl + $"/Content/Remotely-MacOS-{_achitecture}.zip";

                var lastEtag = string.Empty;

                if (File.Exists("etag.txt"))
                {
                    lastEtag = await File.ReadAllTextAsync("etag.txt");
                }

                try
                {
                    var wr = WebRequest.CreateHttp(fileUrl);
                    wr.Method = "Head";
                    wr.Headers.Add("If-None-Match", lastEtag);
                    using var response = (HttpWebResponse)await wr.GetResponseAsync();
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        Logger.Write("Service Updater: aucune mise à jour n'est disponible.");
                        return;
                    }
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotModified)
                {
                    Logger.Write("Service Updater: aucune mise à jour n'est disponible.");
                    return;
                }

                Logger.Write("Service Updater: mise à jour disponible.");

                await InstallLatestVersion();

            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            finally
            {
                _checkForUpdatesLock.Release();
            }
        }

        public void Dispose()
        {
            _webClientEx?.Dispose();
        }

        public async Task InstallLatestVersion()
        {
            try
            {
                await _installLatestVersionLock.WaitAsync();

                var connectionInfo = _configService.GetConnectionInfo();
                var serverUrl = connectionInfo.Host;

                Logger.Write("Service Updater: téléchargement du package.");

                var downloadId = Guid.NewGuid().ToString();
                var zipPath = Path.Combine(Path.GetTempPath(), "RemotelyUpdate.zip");

                var installerPath = Path.Combine(Path.GetTempPath(), "RemotelyUpdate.sh");

                await _webClientEx.DownloadFileTaskAsync(
                       serverUrl + $"/API/ClientDownloads/{connectionInfo.OrganizationID}/MacOSInstaller-{_achitecture}",
                       installerPath);

                await _webClientEx.DownloadFileTaskAsync(
                   serverUrl + $"/API/AgentUpdate/DownloadPackage/macos-{_achitecture}/{downloadId}",
                   zipPath);

                (await WebRequest.CreateHttp(serverUrl + $"/api/AgentUpdate/ClearDownload/{downloadId}").GetResponseAsync()).Dispose();

                Logger.Write("Lancement de l'installateur.");

                Process.Start("sudo", $"chmod +x {installerPath}").WaitForExit();

                Process.Start("sudo", $"{installerPath} --path {zipPath}");
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                Logger.Write("Timeout lors du téléchargement.", Shared.Enums.EventType.Warning);
                _lastUpdateFailure = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                _lastUpdateFailure = DateTimeOffset.Now;
            }
            finally
            {
                _installLatestVersionLock.Release();
            }
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await CheckForUpdates();
        }

    }
}
