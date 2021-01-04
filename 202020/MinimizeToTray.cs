using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace _202020
{
    /// Class implementing support for "minimize to tray" functionality.
    public static class MinimizeToTray
    {
        /// Enables "minimize to tray" behavior for the specified Window.
        /// <param name="window">Window to enable the behavior for.</param>
        public static void Enable(MainWindow window)
        {
            // No need to track this instance; its event handlers will keep it alive
            new MinimizeToTrayInstance(window);
        }

        /// Class implementing "minimize to tray" functionality for a Window instance.
        private class MinimizeToTrayInstance
        {
            private MainWindow _window;
            private NotifyIcon _notifyIcon;

            /// Initializes a new instance of the MinimizeToTrayInstance class.
            /// <param name="window">Window instance to attach to.</param>
            public MinimizeToTrayInstance(MainWindow window)
            {
                _window = window;
                _window.StateChanged += new EventHandler(HandleStateChanged);
            }

            /// Handles the Window's StateChanged event.
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleStateChanged(object sender, EventArgs e)
            {
                if (_notifyIcon == null)
                {
                    // Initialize NotifyIcon instance "on demand"
                    _notifyIcon = new NotifyIcon();
                    Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Media/MainWindowTray.ico")).Stream;
                    _notifyIcon.Icon = new Icon(iconStream);
                    _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconClicked);
                }
                // Update copy of Window Title in case it has changed
                _notifyIcon.Text = _window.Title;

                // Show/hide Window and NotifyIcon
                var minimized = (_window.WindowState == System.Windows.WindowState.Minimized);
                _window.ShowInTaskbar = !minimized;
                _notifyIcon.Visible = minimized;
            }

            /// Handles a click on the notify icon or.
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleNotifyIconClicked(object sender, EventArgs e)
            {
                // Restore the Window
                _window.WindowState = System.Windows.WindowState.Normal;
            }
        }
    }
}
