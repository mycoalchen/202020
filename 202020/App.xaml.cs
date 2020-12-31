using System;
using System.Windows;

namespace _202020
{
    public partial class App : Application
    {
        public TimeSpan TimeBetweenBreaks;
        public TimeSpan BreakLength;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.Properties["SettingsOpen"] = false;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _202020.Properties.Settings.Default.Save();
        }
    }
}
