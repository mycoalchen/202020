using System;
using System.Windows;
using Microsoft.Win32;

namespace _202020
{
    public partial class App : Application
    {
        public TimeSpan TimeBetweenBreaks;
        public TimeSpan BreakLength;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.Properties["SettingsOpen"] = false;
            
            // Event that fires when power mode changes
            SystemEvents.PowerModeChanged += OnPowerChange;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _202020.Properties.Settings.Default.Save();
        }

        private void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                // About to resume from suspended state
                case PowerModes.Resume:
                    if (true)
                    {
                        System.Windows.Forms.Application.Restart();
                        System.Windows.Application.Current.Shutdown();
                    }
                    break;
            }
        }
    }
}
