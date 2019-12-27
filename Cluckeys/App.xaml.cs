﻿using System.Threading;
using System.Windows;

namespace Cluckeys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private bool _isFirstInstance;
        private Mutex? _singletonMutex;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            EnsureSingleton();
            if (!_isFirstInstance)
            {
                Shutdown();
                return;
            }

            TrayIconManager.Instance.ShowTrayIcon();
            CluckeysManager.Instance.Start();
        }

        private void EnsureSingleton()
        {
            _singletonMutex = new Mutex(true, "Cluckeys.App.Mutex", out _isFirstInstance);
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (!_isFirstInstance)
                return;

            _singletonMutex?.ReleaseMutex();
            CluckeysManager.Instance.Dispose();
            TrayIconManager.Instance.HideTrayIcon();
        }
    }
}