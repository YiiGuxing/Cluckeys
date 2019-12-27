using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Cluckeys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private bool _isFirstInstance;
        private Mutex? _singletonMutex;
        private NotifyIcon? _icon;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            EnsureSingleton();
            if (!_isFirstInstance)
            {
                Shutdown();
                return;
            }

            _icon = new NotifyIcon
            {
                Text = "Cluckeys",
                Icon = Cluckeys.Resources.app,
                Visible = true
            };

            try
            {
                CluckeysManager.Instance.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                ShowStoppedRunningMessage(exception.Message);
                Shutdown();
            }
        }

        private void EnsureSingleton()
        {
            _singletonMutex = new Mutex(true, "Cluckeys.App.Mutex", out _isFirstInstance);
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (!_isFirstInstance)
                return;

            if (_icon != null)
            {
                _icon.Visible = false;
                _icon = null;
            }

            try
            {
                CluckeysManager.Instance.Stop();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                ShowStoppedRunningMessage(exception.Message);
            }
            finally
            {
                _singletonMutex?.ReleaseMutex();
            }
        }

        private static void ShowStoppedRunningMessage(string message)
        {
            MessageBox.Show(message, "Cluckeys已停止运行", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}