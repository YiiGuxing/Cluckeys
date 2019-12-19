using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Cluckeys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private KeyboardHook _hook = new KeyboardHook();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _hook.OnKeyDownEvent = OnKeyDownEvent;
            _hook.OnKeyTypeEvent = OnKeyTypeEvent;
            _hook.OnKeyUpEvent = OnKeyUpEvent;
            try
            {
                _hook.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show("错误，无法启用Cluckeys！", "Cluckeys", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnKeyDownEvent(KeyboardHook.KeyboardEvent e)
        {
            Console.WriteLine($"OnKeyDownEvent: {e.vkCode}");
        }

        private void OnKeyTypeEvent(KeyboardHook.KeyboardEvent e)
        {
            Console.WriteLine($"OnKeyTypeEvent: {e.vkCode}");
        }

        private void OnKeyUpEvent(KeyboardHook.KeyboardEvent e)
        {
            Console.WriteLine($"OnKeyUpEvent: {e.vkCode}");
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            try
            {
                _hook.Stop();
                _hook = null;
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