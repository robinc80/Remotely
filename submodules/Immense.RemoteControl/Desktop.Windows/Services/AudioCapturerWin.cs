﻿using Immense.RemoteControl.Desktop.Shared.Abstractions;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Immense.RemoteControl.Desktop.Windows.Services
{
    public class AudioCapturerWin : IAudioCapturer
    {
        private readonly List<byte> _tempBuffer = new();
        private readonly WaveFormat _targetFormat;
        private readonly WasapiLoopbackCapture _capturer = new();
        private readonly Stopwatch _sendTimer = new();


        public AudioCapturerWin()
        {
            _targetFormat = new WaveFormat(16000, 8, 1);
            _capturer.DataAvailable += Capturer_DataAvailable;
        }

        public event EventHandler<byte[]>? AudioSampleReady;

        public void ToggleAudio(bool toggleOn)
        {
            if (toggleOn)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        private void Capturer_DataAvailable(object? sender, WaveInEventArgs args)
        {
            try
            {
                if (args.Buffer.All(x => x == 0))
                {
                    return;
                }

                if (args.BytesRecorded > 0)
                {
                    lock (_tempBuffer)
                    {
                        if (!_sendTimer.IsRunning)
                        {
                            _sendTimer.Restart();
                        }

                        _tempBuffer.AddRange(args.Buffer.Take(args.BytesRecorded));

                        if (_tempBuffer.Count > 50_000 ||
                            _sendTimer.Elapsed.TotalMilliseconds > 1000)
                        {
                            _sendTimer.Reset();
                            SendTempBuffer();
                        }
                    }
                }
            }
            catch { }
        }

        private void SendTempBuffer()
        {
            if (_tempBuffer.Count == 0)
            {
                return;
            }

            using var ms1 = new MemoryStream();
            using (var wfw = new WaveFileWriter(ms1, _capturer.WaveFormat))
            {
                wfw.Write(_tempBuffer.ToArray(), 0, _tempBuffer.Count);
            }
            _tempBuffer.Clear();

            // Resample to 16-bit so Firefox will play it.
            using var ms2 = new MemoryStream(ms1.ToArray());
            using var wfr = new WaveFileReader(ms2);
            using var ms3 = new MemoryStream();
            using (var resampler = new MediaFoundationResampler(wfr, _targetFormat))
            {
                WaveFileWriter.WriteWavFileToStream(ms3, resampler);
            }
            AudioSampleReady?.Invoke(this, ms3.ToArray());
        }

        private void Start()
        {
            try
            {
                _sendTimer.Restart();
                _capturer.StartRecording();
            }
            catch { }
        }

        private void Stop()
        {
            _capturer.StopRecording();
            _sendTimer.Stop();
        }
    }
}
