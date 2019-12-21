using System;
using System.Windows;

namespace Cluckeys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                CluckeysManager.Instance.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show("错误，无法启用Cluckeys！", "Cluckeys", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            try
            {
                CluckeysManager.Instance.Stop();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show("错误，无法停用Cluckeys！", "Cluckeys", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}