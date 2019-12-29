using System;
using System.Windows.Forms;

namespace Cluckeys
{
    internal class TrayIconManager
    {
        private const string KeyUpdate = "UPDATE";

        private readonly NotifyIcon _notifyIcon;

        internal static TrayIconManager Instance { get; }

        static TrayIconManager()
        {
            Instance = new TrayIconManager();
        }

        private TrayIconManager()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(new ToolStripLabel($"{Application.ProductName} v{Application.ProductVersion}")
                {Enabled = false});
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Check for Updates...", null,
                (sender, args) => Updater.CheckForUpdates(false), KeyUpdate));

            AddEnabledMenuItem(contextMenu);
            AddRunAtStartupMenuItem(contextMenu);

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Exit", null,
                (sender, args) => System.Windows.Application.Current.Shutdown()));

            _notifyIcon = new NotifyIcon
            {
                Text = Application.ProductName,
                Icon = Resources.app_notify,
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

        public static void ShowUpdatesAvailable(string version)
        {
            var notifyIcon = Instance._notifyIcon;
            notifyIcon.Icon = Resources.app_notify_new;

            var items = notifyIcon.ContextMenuStrip.Items;
            var index = items.IndexOfKey(KeyUpdate);
            items.RemoveByKey(KeyUpdate);

            var updates = $"{Application.ProductName} {version} is now available!";
            var content = $"{updates} Click here to open the download page.";
            var toolStripLabel = new ToolStripLabel("", null, true,
                (sender, args) => Updater.OpenReleasesPageUrl())
            {
                AutoSize = true,
                ToolTipText = content
            };
            items.Insert(index, toolStripLabel);
            toolStripLabel.Text = updates; // 修复尺寸，初始化的时候设置会导致尺寸异常。

            ShowNotification("", content, timeout: 20000, clickEvent: Updater.OpenReleasesPageUrl);
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