using System;

namespace Cluckeys
{
    internal class CluckeysManager
    {
        private readonly KeyboardHook _keyboardHook;

        public static CluckeysManager Instance { get; }

        static CluckeysManager()
        {
            Instance = new CluckeysManager();
        }

        private CluckeysManager()
        {
            _keyboardHook = new KeyboardHook
            {
                OnKeyDownEvent = OnKeyDownEvent,
                OnKeyTypeEvent = OnKeyTypeEvent,
                OnKeyUpEvent = OnKeyUpEvent
            };
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

        public void Start()
        {
            _keyboardHook.Start();
        }

        public void Stop()
        {
            _keyboardHook.Stop();
        }

        ~CluckeysManager()
        {
            Stop();
        }
    }
}