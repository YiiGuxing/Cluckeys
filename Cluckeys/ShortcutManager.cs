using System;
using System.Collections.Generic;

namespace Cluckeys
{
    public class ShortcutManager : IDisposable
    {
        private bool _isSetup;
        private readonly Dictionary<int, Action> _actions = new Dictionary<int, Action>();

        internal static ShortcutManager Instance { get; }

        static ShortcutManager()
        {
            Instance = new ShortcutManager();
        }

        private ShortcutManager()
        {
            KeyboardHook.Instance.KeyDown += OnKeyDownEvent;
        }

        private void OnKeyDownEvent(KeyboardHook.KeyboardEvent e)
        {
            _actions.GetValueOrDefault(e.code)?.Invoke();
        }

        public void Setup()
        {
            if (_isSetup) return;

            // Ctrl + Shift + F12
            _actions[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 123] = () =>
            {
                var cluckeysManager = CluckeysManager.Instance;
                if (cluckeysManager.IsRunning)
                {
                    cluckeysManager.Stop();
                }
                else
                {
                    cluckeysManager.Start();
                }
            };

            _isSetup = true;
        }

        public void Dispose()
        {
            _actions.Clear();
            KeyboardHook.Instance.KeyDown -= OnKeyDownEvent;
            GC.SuppressFinalize(this);
        }
    }
}