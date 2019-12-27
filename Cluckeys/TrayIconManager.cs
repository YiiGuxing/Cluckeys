using System;
using System.Windows.Forms;

namespace Cluckeys
{
    internal class TrayIconManager
    {
        private readonly NotifyIcon _notifyIcon;

        internal static TrayIconManager Instance { get; }

        static TrayIconManager()
        {
            Instance = new TrayIconManager();
        }

        private TrayIconManager()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(new ToolStripLabel("Cluckeys") {Enabled = false});
            contextMenu.Items.Add(new ToolStripSeparator());

            AddEnabledMenuItem(contextMenu);
            AddRunAtStartupMenuItem(contextMenu);

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null,
                ((sender, args) => System.Windows.Application.Current.Shutdown())));

            _notifyIcon = new NotifyIcon
            {
                Text = @"Cluckeys",
                Icon = Resources.app_16,
                ContextMenuStrip = contextMenu
            };
        }

        private static void AddEnabledMenuItem(ToolStripDropDown toolStrip)
        {
            var enabledMenuItem = new ToolStripMenuItem("Enabled", null, (sender, args) =>
            {
                if (sender == null) return;
                var menuItem = (ToolStripMenuItem) sender;
                if (menuItem.Checked)
                {
                    CluckeysManager.Instance.Stop();
                }
                else
                {
                    CluckeysManager.Instance.Start();
                }

                menuItem.Checked = !menuItem.Checked;
            });

            toolStrip.Items.Add(enabledMenuItem);
            toolStrip.Opened += (sender, args) => { enabledMenuItem.Checked = CluckeysManager.Instance.IsRunning; };
        }

        private static void AddRunAtStartupMenuItem(ToolStripDropDown toolStrip)
        {
            var runAtStartupMenuItem = new ToolStripMenuItem("Run at Startup", null, (sender, args) =>
            {
                if (sender == null) return;
                var menuItem = (ToolStripMenuItem) sender;
                var result = menuItem.Checked
                    ? AutoStartupHelper.RemoveAutoRunShortcut()
                    : AutoStartupHelper.CreateAutoRunShortcut();
                if (result)
                {
                    menuItem.Checked = !menuItem.Checked;
                }
            });

            toolStrip.Items.Add(runAtStartupMenuItem);
            toolStrip.Opened += (sender, args) => { runAtStartupMenuItem.Checked = AutoStartupHelper.IsAutoRun(); };
        }

        internal void ShowTrayIcon()
        {
            _notifyIcon.Visible = true;
        }

        internal void HideTrayIcon()
        {
            _notifyIcon.Visible = false;
        }

        public static void ShowNotification(string title, string content, bool isError = false, int timeout = 5000,
            Action? clickEvent = null,
            Action? closeEvent = null)
        {
            var icon = Instance._notifyIcon;
            icon.ShowBalloonTip(timeout, title, content, isError ? ToolTipIcon.Error : ToolTipIcon.Info);
            icon.BalloonTipClicked += OnIconOnBalloonTipClicked;
            icon.BalloonTipClosed += OnIconOnBalloonTipClosed;

            void OnIconOnBalloonTipClicked(object? sender, EventArgs e)
            {
                clickEvent?.Invoke();
                icon.BalloonTipClicked -= OnIconOnBalloonTipClicked;
            }

            void OnIconOnBalloonTipClosed(object? sender, EventArgs e)
            {
                closeEvent?.Invoke();
                icon.BalloonTipClosed -= OnIconOnBalloonTipClosed;
            }
        }
    }
}