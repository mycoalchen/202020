using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;

namespace _202020
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TimeSpan TimeRemaining;
        DispatcherTimer countdown;
        bool OnBreak = false;
        MediaPlayer Notif;

        Window w; // helper window used to hide from alt-tab menu

        // Hotkey stuff
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource _source;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_WIN = 0x0008;
        private const int PLAYPAUSE_ID = 9000;
        private const int FASTFORWARD_ID = 9001;
        private const int KEY_RIGHT = 0x27;
        private const int KEY_SPACE = 0x20;

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case PLAYPAUSE_ID: // Play/Pause
                            if (Properties.Settings.Default.PlayPauseShortcut)
                            {
                                if (countdown.IsEnabled)
                                {
                                    countdown.IsEnabled = false;
                                    PlayPauseImage.Source = new BitmapImage(new Uri(@"/Media/Play.png", UriKind.Relative));
                                }
                                else
                                {
                                    countdown.IsEnabled = true;
                                    PlayPauseImage.Source = new BitmapImage(new Uri(@"/Media/Pause.png", UriKind.Relative));
                                }
                                handled = true;
                            }
                            break;
                        case FASTFORWARD_ID: // Fast forward
                            if (Properties.Settings.Default.FastForwardShortcut)
                            {
                                if (OnBreak)
                                { // end break, start TBB timer
                                    EndBreak();
                                    foreach (Window window in System.Windows.Application.Current.Windows)
                                    {
                                        if (window.GetType() == typeof(BreakNotification))
                                            window.Close();
                                    }
                                    if (Properties.Settings.Default.NotificationStopSound &&
                                            Properties.Settings.Default.NotificationsEnabled)
                                    {
                                        Notif.Volume = 0.005 * Properties.Settings.Default.StopVolume;
                                        Notif.Play(); Notif.Position = new TimeSpan(0, 0, 0);
                                    }
                                }
                                else
                                { // start break
                                    StartBreak(Properties.Settings.Default.NotificationsEnabled);
                                    if (Properties.Settings.Default.NotificationStopSound && Properties.Settings.Default.NotificationsEnabled)
                                    {
                                        Notif.Volume = 0.005 * Properties.Settings.Default.StartVolume;
                                        Notif.Play(); Notif.Position = new TimeSpan(0, 0, 0);
                                    }
                                }
                                UpdateTimerText();
                                handled = true;
                            }
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(handle);
            _source.AddHook(HwndHook);

            if (Properties.Settings.Default.PlayPauseShortcut)
                RegisterHotKey(handle, PLAYPAUSE_ID, MOD_ALT | MOD_WIN, KEY_SPACE);
            if (Properties.Settings.Default.FastForwardShortcut)
                RegisterHotKey(handle, FASTFORWARD_ID, MOD_ALT | MOD_WIN, KEY_RIGHT);

        }

        public MainWindow()
        {
            InitializeComponent();

            if (!Properties.Settings.Default.RunInTaskbar)
            {
                MinimizeToTray.Enable(this);
                if (!Properties.Settings.Default.ShowInAltTab)
                {
                    w = new Window();
                    w.Top = -100; // outside visible part of screen
                    w.Left = -100;
                    w.Width = 1; // size is small enough to avoid initial appearance
                    w.Height = 1;
                    w.WindowStyle = WindowStyle.ToolWindow;
                    w.ShowInTaskbar = false;
                    w.Show();
                    Owner = w;
                    w.Hide();
                }
            }
            else
            {
                ShowInTaskbar = true;
            }

            TimeRemaining = new TimeSpan(Properties.Settings.Default.TBBhours,
                Properties.Settings.Default.TBBminutes,
                Properties.Settings.Default.TBBseconds);

            Notif = new MediaPlayer();
            Notif.Open(new Uri(@"../../Media/Doot.wav", UriKind.Relative));

            countdown = new DispatcherTimer(DispatcherPriority.Normal);
            countdown.Tick += OnTick;
            countdown.Interval = new TimeSpan(0, 0, 1);
            countdown.Start();
        }
        private void OnTick(object sender, EventArgs e)
        {
            if (TimeRemaining.TotalSeconds == 0.0)
            {
                if (OnBreak)
                { // end break, start TBB timer
                    EndBreak();
                    if (Properties.Settings.Default.NotificationStopSound && Properties.Settings.Default.NotificationsEnabled)
                    {
                        Notif.Volume = 0.005 * Properties.Settings.Default.StopVolume;
                        Notif.Play(); Notif.Position = new TimeSpan(0, 0, 0);
                    }
                }
                else
                { // start break
                    StartBreak(Properties.Settings.Default.NotificationsEnabled);
                    if (Properties.Settings.Default.NotificationStartSound && Properties.Settings.Default.NotificationsEnabled)
                    {
                        Notif.Volume = 0.005 * Properties.Settings.Default.StartVolume;
                        Notif.Play(); Notif.Position = new TimeSpan(0, 0, 0);
                    }
                }
            }
            UpdateTimerText();
        }
        public void UpdateTimerText()
        {

            TimeRemaining = TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
            HoursRemaining.Text = TimeRemaining.Hours.ToString("D2");
            MinutesRemaining.Text = TimeRemaining.Minutes.ToString("D2");
            SecondsRemaining.Text = TimeRemaining.Seconds.ToString("D2");
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.Properties["SettingsOpen"].Equals(false))
            {
                var nextWin = new Settings();
                nextWin.Show();
                System.Windows.Application.Current.Properties["SettingsOpen"] = true;
            }
        }
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (countdown.IsEnabled)
            {
                countdown.Stop();
                PlayPauseImage.Source = new BitmapImage(new Uri(@"/Media/Play.png", UriKind.Relative));
            }
            else
            {
                countdown.Start();
                PlayPauseImage.Source = new BitmapImage(new Uri(@"/Media/Pause.png", UriKind.Relative));
            }
        }
        private void FastForward_Click(object sender, RoutedEventArgs e)
        {
            if (OnBreak) // end break, start TBB timer
                EndBreak();
            else // start break
                StartBreak(Properties.Settings.Default.NotificationsEnabled);
            UpdateTimerText();
        }
        private void Information_Click(object sender, RoutedEventArgs e)
        {
            var InfoWin = new Information();
            InfoWin.Show();
        }
        public void StartBreak(bool ShowNotification)
        {
            TimeRemaining = TimeSpan.FromSeconds(
                Properties.Settings.Default.BLseconds +
                60 * Properties.Settings.Default.BLminutes +
                3600 * Properties.Settings.Default.BLhours);

            HoursRemaining.Foreground = System.Windows.Media.Brushes.Black;
            MinutesRemaining.Foreground = System.Windows.Media.Brushes.Black;
            SecondsRemaining.Foreground = System.Windows.Media.Brushes.Black;
            Colon1.Foreground = System.Windows.Media.Brushes.Black;
            Colon2.Foreground = System.Windows.Media.Brushes.Black;

            if (ShowNotification && Properties.Settings.Default.NotificationTextEnabled == true)
            {
                var NotifWin = new BreakNotification();
                NotifWin.Show(); NotifWin.Activate();
            }
            OnBreak = true;
        }
        public void EndBreak()
        {
            TimeRemaining = TimeSpan.FromSeconds(
                Properties.Settings.Default.TBBseconds +
                60 * Properties.Settings.Default.TBBminutes +
                3600 * Properties.Settings.Default.TBBhours);

            HoursRemaining.Foreground = System.Windows.Media.Brushes.White;
            MinutesRemaining.Foreground = System.Windows.Media.Brushes.White;
            SecondsRemaining.Foreground = System.Windows.Media.Brushes.White;
            Colon1.Foreground = System.Windows.Media.Brushes.White;
            Colon2.Foreground = System.Windows.Media.Brushes.White;

            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.GetType() == typeof(BreakNotification))
                { window.Close(); }
            }
            OnBreak = false;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (w != null)
            {
                w.Close();
            }
            base.OnClosing(e);
        }
    }
}
