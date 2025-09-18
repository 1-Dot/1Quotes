using System.Configuration;
using System.Data;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Input;

namespace _1Quotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon? _trayIcon;
        private KeyboardHookManager? _keyboardHook;
        public static readonly ICommand TrayOpenCommand = new RoutedCommand();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsManager.Load();
            _keyboardHook = new KeyboardHookManager
            {
                GetTriggerMode = () => SettingsManager.Current.TriggerMode
            };

            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            CommandManager.RegisterClassCommandBinding(typeof(App), new CommandBinding(TrayOpenCommand, (_, __) => ShowMainWindow()));
        }

        private void ShowMainWindow()
        {
            if (Current.MainWindow == null)
            {
                var win = new MainWindow();
                win.Show();
            }
            else
            {
                if (Current.MainWindow.WindowState == WindowState.Minimized)
                    Current.MainWindow.WindowState = WindowState.Normal;
                Current.MainWindow.Show();
                Current.MainWindow.Activate();
            }
        }

        private void OnTrayOpenClick(object sender, RoutedEventArgs e) => ShowMainWindow();

        private void OnTrayExitClick(object sender, RoutedEventArgs e) => Shutdown();

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) => ShowMainWindow();

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
            _keyboardHook?.Dispose();
            _keyboardHook = null;
            base.OnExit(e);
        }
    }

}
