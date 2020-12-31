using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace _202020
{
    /// <summary>
    /// Class implementing support for "minimize to tray" functionality.
    /// </summary>
    public static class MinimizeToTray
    {
        /// <summary>
        /// Enables "minimize to tray" behavior for the specified Window.
        /// </summary>
        /// <param name="window">Window to enable the behavior for.</param>
        public static void Enable(MainWindow window)
        {
            // No need to track this instance; its event handlers will keep it alive
            new MinimizeToTrayInstance(window);
        }

        /// <summary>
        /// Class implementing "minimize to tray" functionality for a Window instance.
        /// </summary>
        private class MinimizeToTrayInstance
        {
            private MainWindow _window;
            private NotifyIcon _notifyIcon;

            /// <summary>
            /// Initializes a new instance of the MinimizeToTrayInstance class.
            /// </summary>
            /// <param name="window">Window instance to attach to.</param>
            public MinimizeToTrayInstance(MainWindow window)
            {
                _window = window;
                _window.StateChanged += new EventHandler(HandleStateChanged);
            }

            /// <summary>
            /// Handles the Window's StateChanged event.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleStateChanged(object sender, EventArgs e)
            {
                if (_notifyIcon == null)
                {
                    // Initialize NotifyIcon instance "on demand"
                    _notifyIcon = new NotifyIcon();
                    _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                    _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconClicked);
                }
                // Update copy of Window Title in case it has changed
                _notifyIcon.Text = _window.Title;

                // Show/hide Window and NotifyIcon
                var minimized = (_window.WindowState == System.Windows.WindowState.Minimized);
                _window.ShowInTaskbar = !minimized;
                _notifyIcon.Visible = minimized;
            }

            /// <summary>
            /// Handles a click on the notify icon or.
            /// </summary>
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
