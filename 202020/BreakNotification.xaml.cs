using System.Windows;

namespace _202020
{
    /// <summary>
    /// Interaction logic for BreakNotification.xaml
    /// </summary>
    public partial class BreakNotification : Window
    {
        public BreakNotification()
        {
            InitializeComponent();

            NotifMessage.Text = Properties.Settings.Default.NotificationText;
            if (Properties.Settings.Default.FullScreenNotifications)
            {
                SizeToContent = SizeToContent.Manual;
                WindowState = WindowState.Maximized;
                NotifMessage.FontSize = 28;
                NotifMessage.Text += "\n\nPress Escape to close this window, or it will close itself when the timer ends.";
            } else
            {
                SizeToContent = SizeToContent.Height;
                ResizeMode = ResizeMode.CanMinimize;
                NotifMessage.FontSize = 16;
            }
            this.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(HandleEsc);
        }
        private void HandleEsc(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape) Close();
        }
    }
}
