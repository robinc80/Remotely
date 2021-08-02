using Remotely.Agent.Installer.Win.Utilities;
using Remotely.Agent.Installer.Win.ViewModels;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Remotely.Agent.Installer.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (CommandLineParser.CommandLineArgs.ContainsKey("quiet"))
            {
                Hide();
                ShowInTaskbar = false;
                _ = new MainWindowViewModel().Init();
            }
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await (DataContext as MainWindowViewModel).Init();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ShowServerUrlHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "URL du serveur Remotely à utiliser pour la connexion.", 
                "URL du serveur", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        private void ShowOrganizationIdHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Ceci est l'ID d'organisation.  Comme Remotely supporte les connexions multiples, " +
                "cet identifiant doit être renseigné pour savoir qui peut avoir accès à cet ordinateur." 
                + Environment.NewLine + Environment.NewLine +
                "Cet ID se trouve sur la page du serveur Remotely.", 
                "ID d'organisation", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        private void ShowSupportShortcutHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Si la case est cochée, un raccourci vous permettant de demander de l'aide sera créé sur le Bureau.", 
                "Raccourci",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
