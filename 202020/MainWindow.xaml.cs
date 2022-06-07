using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using System.Diagnostics;
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
        MediaPlayer StartNotif, StopNotif;
        MediaPlayer PlayNotif, PauseNotif;
        BreakNotification NotifWin;

        Window w; // helper window used to hide from alt-tab menu

        #region Hotkey stuff
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource _source;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_WIN = 0x0008;
        private const int PLAYPAUSE_ID = 9000;
        private const int FASTFORWARD_ID = 9001;
        private const int KEY_X = 0x58;
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
                                    if ((bool)Properties.Settings.Default.PlayPauseSound)
                                    {
                                        PlayPauseNotifPlay(false);
                                    }
                                    countdown.IsEnabled = false;
                                    PlayPauseImage.Source = new BitmapImage(new Uri(@"/Media/Play.png", UriKind.Relative));
                                }
                                else
                                {
                                    if ((bool)Properties.Settings.Default.PlayPauseSound)
                                    {
                                        PlayPauseNotifPlay(true);
                                    }
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
                                        NotifPlay(false);
                                    }
                                }
                                else
                                { // start break
                                    StartBreak(Properties.Settings.Default.NotificationsEnabled);
                                    if (Properties.Settings.Default.NotificationStopSound && Properties.Settings.Default.NotificationsEnabled)
                                    {
                                        NotifPlay(true);
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
                RegisterHotKey(handle, FASTFORWARD_ID, MOD_ALT | MOD_WIN, KEY_X);

        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            MainTimeToolTip.Text = "Time until next break";
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
            else { ShowInTaskbar = true; }

            TimeRemaining = new TimeSpan(Properties.Settings.Default.TBBhours,
                Properties.Settings.Default.TBBminutes,
                Properties.Settings.Default.TBBseconds + 1);

            // Add sounds to manifest resource stream
            using (FileStream fileStream = File.Create(Path.GetTempPath() + "StartBreak.wav"))
            {
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("_202020.Media.StartBreak.wav").CopyTo(fileStream);
            }
            using (FileStream fileStream = File.Create(Path.GetTempPath() + "StopBreak.wav"))
            {
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("_202020.Media.StopBreak.wav").CopyTo(fileStream);
            }
            using (FileStream fileStream = File.Create(Path.GetTempPath() + "Play.wav"))
            {
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("_202020.Media.Play.wav").CopyTo(fileStream);
            }
            using (FileStream fileStream = File.Create(Path.GetTempPath() + "Pause.wav"))
            {
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("_202020.Media.Pause.wav").CopyTo(fileStream);
            }

            countdown = new DispatcherTimer(DispatcherPriority.Normal);
            countdown.Tick += OnTick;
            countdown.Interval = new TimeSpan(0, 0, 1);
            countdown.Start();

            StartNotif = new MediaPlayer();
            StartNotif.Open(new Uri(Path.Combine(Path.GetTempPath(), "StartBreak.wav")));
            StopNotif = new MediaPlayer();
            StopNotif.Open(new Uri(Path.Combine(Path.GetTempPath(), "StopBreak.wav")));
            PlayNotif = new MediaPlayer();
            PlayNotif.Open(new Uri(Path.Combine(Path.GetTempPath(), "Play.wav")));
            PauseNotif = new MediaPlayer();
            PauseNotif.Open(new Uri(Path.Combine(Path.GetTempPath(), "Pause.wav")));

        }

        

        private void NotifPlay(bool Start)
        {
            Debug.WriteLine("NotifPlay called");
            if (Start)
            {
                StartNotif.MediaFailed += Notif_MediaFailed;
                StartNotif.Volume = Start ? 0.04 * Properties.Settings.Default.StartVolume : 0.04 * Properties.Settings.Default.StopVolume;
                StartNotif.Position = new TimeSpan(0, 0, 0);
                StartNotif.Play();
            }
            else
            {
                StopNotif.MediaFailed += Notif_MediaFailed;
                StopNotif.Volume = Start ? 0.04 * Properties.Settings.Default.StartVolume : 0.04 * Properties.Settings.Default.StopVolume;
                StopNotif.Position = new TimeSpan(0, 0, 0);
                StopNotif.Play();
            }
        }
        private void PlayPauseNotifPlay(bool Play)
        {
            Debug.WriteLine("PlayPauseNotifPlay called");
            if (Play)
            {
                PlayNotif.MediaFailed += Notif_MediaFailed;
                PlayNotif.Volume = 0.02 * Properties.Settings.Default.StartVolume;
                PlayNotif.Position = new TimeSpan(0, 0, 0);
                PlayNotif.Play();
            }
            else
            {
                PauseNotif.MediaFailed += Notif_MediaFailed;
                PauseNotif.Volume = 0.02 * Properties.Settings.Default.StopVolume;
                PauseNotif.Position = new TimeSpan(0, 0, 0);
                PauseNotif.Play();
            }
        }

        private void Notif_MediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show("Error- failed to open notification sound");
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
                        NotifPlay(false);
                    }
                    Debug.WriteLine("Finished break");
                }
                else
                { // start break
                    StartBreak(Properties.Settings.Default.NotificationsEnabled);
                    if (Properties.Settings.Default.NotificationStartSound && Properties.Settings.Default.NotificationsEnabled)
                    {
                        NotifPlay(true);
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
                3600 * Properties.Settings.Default.BLhours + 1);

            HoursRemaining.Foreground = Brushes.Black;
            MinutesRemaining.Foreground = Brushes.Black;
            SecondsRemaining.Foreground = Brushes.Black;
            Colon1.Foreground = Brushes.Black;
            Colon2.Foreground = Brushes.Black;

            if (ShowNotification && Properties.Settings.Default.NotificationTextEnabled == true)
            {
                NotifWin = new BreakNotification();
                NotifWin.Show(); 
                if (Properties.Settings.Default.NotificationFocused) { NotifWin.Activate(); }
            }
            MainTimeToolTip.Text = "Time until break ends";
            OnBreak = true;
        }
        public void EndBreak()
        {
            TimeRemaining = TimeSpan.FromSeconds(
                Properties.Settings.Default.TBBseconds +
                60 * Properties.Settings.Default.TBBminutes +
                3600 * Properties.Settings.Default.TBBhours + 1);

            HoursRemaining.Foreground = Brushes.White;
            MinutesRemaining.Foreground = Brushes.White;
            SecondsRemaining.Foreground = Brushes.White;
            Colon1.Foreground = Brushes.White;
            Colon2.Foreground = Brushes.White;

            if (NotifWin != null)
            {
                NotifWin.Close();
            }
            MainTimeToolTip.Text = "Time until next break";
            OnBreak = false;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (w != null)
            {
                w.Close();
            }
            if (NotifWin != null)
            {
                NotifWin.Close();
            }
            base.OnClosing(e);
        }
    }
}
