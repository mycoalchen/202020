using System;
using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace _202020
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public class TwoDigit : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(value.ToString()).ToString("D2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public partial class Settings : Window
    {
        public string _TBBhours
        {
            get { return Properties.Settings.Default.TBBhours.ToString("D2"); }
        }
        public string _TBBminutes
        {
            get { return Properties.Settings.Default.TBBminutes.ToString("D2"); }
        }
        public string _TBBseconds
        {
            get { return Properties.Settings.Default.TBBseconds.ToString("D2"); }
        }
        public string _BLhours
        {
            get { return Properties.Settings.Default.BLhours.ToString("D2"); }
        }
        public string _BLminutes
        {
            get { return Properties.Settings.Default.BLminutes.ToString("D2"); }

        }
        public string _BLseconds
        {
            get { return Properties.Settings.Default.BLseconds.ToString("D2"); }
        }

        public Settings()
        {
            InitializeComponent();
        }

        private void SaveClose_Click(object sender, RoutedEventArgs e)
        {
            // Check if TimeBetweenBreaks and BreakLength numbers are valid
            int __TBBhours, __TBBminutes, __TBBseconds, __BLhours, __BLminutes, __BLseconds;
            if (!int.TryParse(TBBhours.Text, out __TBBhours) || __TBBhours < 0 || __TBBhours > 99 ||
                !int.TryParse(TBBminutes.Text, out __TBBminutes) || __TBBminutes < 0 || __TBBminutes > 59 ||
                !int.TryParse(TBBseconds.Text, out __TBBseconds) || __TBBseconds < 0 || __TBBseconds > 59 ||
                !int.TryParse(BLhours.Text, out __BLhours) || __BLhours < 0 || __BLhours > 99 ||
                !int.TryParse(BLminutes.Text, out __BLminutes) || __BLminutes < 0 || __BLminutes > 59 ||
                !int.TryParse(BLseconds.Text, out __BLseconds) || __BLseconds < 0 || __BLseconds > 59)
            {
                MessageBox.Show("Invalid time input: hours must be between 0 and 99, minutes and seconds must be between 0 and 59", "Time input error");
                return;
            }
            if ((__TBBhours == 0 && __TBBminutes == 0 && __TBBseconds == 0)
                || (__BLhours == 0 && __BLminutes == 0 && __BLseconds == 0))
            {
                MessageBox.Show("Invalid time input: cannot have time length of 0", "Time input error");
                return;
            }

            // TBB and BL
            Properties.Settings.Default.TBBhours = __TBBhours;
            Properties.Settings.Default.TBBminutes = __TBBminutes;
            Properties.Settings.Default.TBBseconds = __TBBseconds;
            Properties.Settings.Default.BLhours = __BLhours;
            Properties.Settings.Default.BLminutes = __BLminutes;
            Properties.Settings.Default.BLseconds = __BLseconds;

            // Notifications
            Properties.Settings.Default.NotificationsEnabled = (bool)NotificationsEnabled.IsChecked;
            Properties.Settings.Default.NotificationStartSound = (bool)StartSound.IsChecked;
            Properties.Settings.Default.NotificationStopSound = (bool)StopSound.IsChecked;
            Properties.Settings.Default.NotificationTextEnabled = (bool)NotificationTextEnabled.IsChecked;
            Properties.Settings.Default.NotificationText = NotificationTextMessage.Text;
            int _StartVolume, _StopVolume, _PlayPauseVolume;
            if (!int.TryParse(StartVolume.Text, out _StartVolume) || _StartVolume < 1 || _StartVolume > 100)
            {
                MessageBox.Show("Invalid start volume- must be between 1 and 100");
                return;
            }
            if (!int.TryParse(StopVolume.Text, out _StopVolume) || _StopVolume < 1 || _StopVolume > 100)
            {
                MessageBox.Show("Invalid stop volume- must be between 1 and 100");
                return;
            }
            if (!int.TryParse(PauseResumeVolume.Text, out _PlayPauseVolume) || _PlayPauseVolume < 1 || _PlayPauseVolume > 100)
            {
                MessageBox.Show("Invalid pause/resume volume- must be between 1 and 100");
            }
            Properties.Settings.Default.StartVolume = _StartVolume;
            Properties.Settings.Default.StopVolume = _StopVolume;
            Properties.Settings.Default.PlayPauseVolume = _PlayPauseVolume;
            Properties.Settings.Default.PlayPauseSound = (bool)PlayPauseSoundEnabled.IsChecked;

            // Run in taskbar
            Properties.Settings.Default.RunInTaskbar = (bool)RunInTaskbar.IsChecked;
            Properties.Settings.Default.ShowInAltTab = (bool)ShowInAltTab.IsChecked || (bool)RunInTaskbar.IsChecked;
            Properties.Settings.Default.Save();

            Application.Current.Properties["SettingsOpen"] = false;
            this.Close();
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            TBBhours.Text = "00";
            TBBminutes.Text = "20";
            TBBseconds.Text = "00";
            BLhours.Text = "00";
            BLminutes.Text = "00";
            BLseconds.Text = "20";
            NotificationsEnabled.IsChecked = true;
            NotificationTextEnabled.IsChecked = true;
            NotificationTextMessage.Text = "Look at something 20 feet away for 20 seconds!";
            StartSound.IsChecked = true;
            StopSound.IsChecked = true;
            RunInTaskbar.IsChecked = false;
            StartVolume.Text = "50";
            StopVolume.Text = "50";
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Current.Properties["SettingsOpen"] = false;
            base.OnClosing(e);
        }

        private void RunInTaskbar_Click(object sender, RoutedEventArgs e)
        {
            ShowInAltTab.IsChecked = (bool)RunInTaskbar.IsChecked ? true : ShowInAltTab.IsChecked;
        }
    }
}
