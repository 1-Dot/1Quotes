using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;

namespace _1Quotes
{
    public partial class MainWindow : Window
    {
        private const string StartupRegPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AppName = "1Quotes";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            StartupCheckBox.Checked += StartupCheckBox_Changed;
            StartupCheckBox.Unchecked += StartupCheckBox_Changed;
            InputModeComboBox.SelectionChanged += InputModeComboBox_SelectionChanged;
            RepoButton.Click += RepoButton_Click;
        }

        private void RepoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/1-Dot/1Quotes",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartupCheckBox.IsChecked = SettingsManager.Current.RunAtStartup && IsStartupRegistered();
            InputModeComboBox.SelectedIndex = SettingsManager.Current.TriggerMode == InputTriggerMode.ShiftBracket ? 0 : 1;

            if (!IsAdministrator())
            {
                StartupCheckBox.IsEnabled = false;
            }
        }

        private void InputModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            SettingsManager.Current.TriggerMode = InputModeComboBox.SelectedIndex == 0 ? InputTriggerMode.ShiftBracket : InputTriggerMode.BracketOnly;
            SettingsManager.Save();
        }

        private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            bool want = StartupCheckBox.IsChecked == true;
            if (want) RegisterStartup(); else UnregisterStartup();
            SettingsManager.Current.RunAtStartup = want;
            SettingsManager.Save();
        }

        private bool IsAdministrator()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        private bool IsStartupRegistered()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(StartupRegPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch { return false; }
        }

        private void RegisterStartup()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(StartupRegPath, true);
                key?.SetValue(AppName, Process.GetCurrentProcess().MainModule!.FileName);
            }
            catch { }
        }

        private void UnregisterStartup()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(StartupRegPath, true);
                if (key?.GetValue(AppName) != null)
                    key.DeleteValue(AppName, false);
            }
            catch { }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 改为隐藏到托盘
            e.Cancel = true;
            Hide();
        }
    }
}