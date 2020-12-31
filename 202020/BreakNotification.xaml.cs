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
        }
    }
}
