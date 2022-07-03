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
            } else
            {
                SizeToContent = SizeToContent.Height;
                ResizeMode = ResizeMode.CanMinimize;
                NotifMessage.FontSize = 16;
            }
        }
    }
}
