// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cluckeys
{
    /// <summary>
    /// Keyboard hook.
    /// 
    /// Docs: https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        public const int VK_WINDOWS_L = 91; // Left Windows
        public const int VK_WINDOWS_R = 92; // Right Windows
        public const int VK_SHIFT_L = 160; // Left Shift
        public const int VK_SHIFT_R = 161; // Right Shift
        public const int VK_CTRL_L = 162; // Left Ctrl
        public const int VK_CTRL_R = 163; // Right Ctrl
        public const int VK_ALT_L = 164; // Left Alt
        public const int VK_ALT_R = 165; // Right Alt

        public const int WINDOWS_MASK = 1 << 8;
        public const int SHIFT_MASK = 1 << 9;
        public const int CTRL_MASK = 1 << 10;
        public const int ALT_MASK = 1 << 11;

        /// <summary>
        /// WINDOWS + L
        /// </summary>
        public const int LOCK_SHORTCUT = WINDOWS_MASK | 76;

        // Installs a hook procedure that monitors low-level keyboard input events.
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        // The keyboard hook handle.
        private int _keyboardHookHandle;

        private int _modifiers;

        private readonly KeyboardHookStructure eventStructureTemp = new KeyboardHookStructure();

        private readonly Dictionary<int, int> _eventDict = new Dictionary<int, int>();

        internal event OnKeyEvent? KeyDown;
        internal event OnKeyEvent? KeyType;
        internal event OnKeyEvent? KeyUp;


        internal static KeyboardHook Instance { get; }

        static KeyboardHook()
        {
            Instance = new KeyboardHook();
        }

        protected KeyboardHook()
        {
            Hook();
        }

        public delegate void OnKeyEvent(KeyboardEvent e);

        // https://docs.microsoft.com/zh-cn/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
        private delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// Installs an application-defined hook procedure into a hook chain.
        /// You would install a hook procedure to monitor the system for certain types of events.
        /// These events are associated either with a specific thread or with all threads in the
        /// same desktop as the calling thread.
        ///
        /// https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        /// </summary>
        /// <param name="idHook">The type of hook procedure to be installed.</param>
        /// <param name="lpFn"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpFn, IntPtr hInstance, int threadId);


        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the <see cref="SetWindowsHookEx"/> function.
        /// </summary>
        /// <param name="hhk">
        /// A handle to the hook to be removed.
        /// This parameter is a hook handle obtained by a previous call to <see cref="SetWindowsHookEx"/>.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int hhk);


        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information.
        ///
        /// https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-callnexthookex
        /// </summary>
        /// <param name="hhk"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int hhk, int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// 使用 WINDOWS API 函数代替获取当前实例的函数，防止钩子失效。
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);

        private void Hook()
        {
            if (_keyboardHookHandle != 0) return;

            // 安装键盘钩子 
            var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var moduleHandle = GetModuleHandle(processModule.ModuleName);
                _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, moduleHandle, 0);
            }

            if (_keyboardHookHandle == 0)
            {
                Console.WriteLine(@"Keyboard hook installation failed");
            }
        }

        private KeyboardEvent? GetKeyboardEvent(IntPtr lParam)
        {
            eventStructureTemp.Reset();
            Marshal.PtrToStructure(lParam, eventStructureTemp);
            return eventStructureTemp.vkCode > 0 ? new KeyboardEvent(eventStructureTemp, _modifiers) : null;
        }

        private int LowLevelKeyboardProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            KeyboardEvent? keyboardEvent;
            if (nCode >= 0 && (keyboardEvent = GetKeyboardEvent(lParam)) != null)
            {
                var vkCode = keyboardEvent.vkCode;
                switch (wParam)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                    {
                        var count = _eventDict.GetValueOrDefault(vkCode, 0) + 1;
                        _eventDict[vkCode] = count;
                        if (count == 1)
                        {
                            AddModifier(vkCode);
                            KeyDown?.Invoke(keyboardEvent);
                        }
                        else
                        {
                            KeyType?.Invoke(keyboardEvent);
                        }

                        // 锁定Windows以后就收不到事件了
                        if (keyboardEvent.code == LOCK_SHORTCUT)
                        {
                            foreach (var (_vkCode, _) in _eventDict)
                            {
                                RemoveModifier(_vkCode);
                                KeyUp?.Invoke(new KeyboardEvent(_vkCode, _modifiers));
                            }

                            _eventDict.Clear();
                        }

                        break;
                    }
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        // 解锁Windows以后收到事件就丢弃掉
                        if (_eventDict.Count == 0)
                        {
                            _modifiers = 0;
                            break;
                        }

                        RemoveModifier(vkCode);
                        _eventDict.Remove(vkCode);
                        KeyUp?.Invoke(keyboardEvent);
                        break;
                }
            }

            // 如果返回1，则结束消息，这个消息到此为止，不再传递。
            // 如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者。
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        private void AddModifier(int vkCode)
        {
            switch (vkCode)
            {
                case VK_WINDOWS_L:
                case VK_WINDOWS_R:
                    _modifiers |= WINDOWS_MASK;
                    break;
                case VK_SHIFT_L:
                case VK_SHIFT_R:
                    _modifiers |= SHIFT_MASK;
                    break;
                case VK_CTRL_L:
                case VK_CTRL_R:
                    _modifiers |= CTRL_MASK;
                    break;
                case VK_ALT_L:
                case VK_ALT_R:
                    _modifiers |= ALT_MASK;
                    break;
            }
        }

        private void RemoveModifier(int vkCode)
        {
            switch (vkCode)
            {
                case VK_WINDOWS_L:
                case VK_WINDOWS_R:
                    _modifiers &= ~WINDOWS_MASK;
                    break;
                case VK_SHIFT_L:
                case VK_SHIFT_R:
                    _modifiers &= ~SHIFT_MASK;
                    break;
                case VK_CTRL_L:
                case VK_CTRL_R:
                    _modifiers &= ~CTRL_MASK;
                    break;
                case VK_ALT_L:
                case VK_ALT_R:
                    _modifiers &= ~ALT_MASK;
                    break;
            }
        }

        private void Unhook()
        {
            if (_keyboardHookHandle == 0) return;
            if (!UnhookWindowsHookEx(_keyboardHookHandle))
            {
                Console.WriteLine($@"Unable to unhook keyboard hook: {_keyboardHookHandle}");
            }

            _modifiers = 0;
            _eventDict.Clear();
            _keyboardHookHandle = 0;
        }

        public void Dispose()
        {
            Unhook();
            GC.SuppressFinalize(this);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class KeyboardHookStructure
        {
            /// <summary>
            /// 虚拟键码，其值的范围是1至254
            /// </summary>
            internal int vkCode;

            internal KeyboardHookStructure()
            {
                vkCode = 0;
            }

            internal void Reset()
            {
                vkCode = 0;
            }
        }

        /// <summary>
        /// 键盘事件结构
        /// </summary>
        public class KeyboardEvent
        {
            /// <summary>
            /// 虚拟键码，其值的范围是1至254
            /// </summary>
            public readonly int vkCode;

            private readonly int _modifiers;

            public int modifiers
            {
                get
                {
                    return vkCode switch
                    {
                        VK_WINDOWS_L => (_modifiers & ~WINDOWS_MASK),
                        VK_WINDOWS_R => (_modifiers & ~WINDOWS_MASK),
                        VK_SHIFT_L => (_modifiers & ~SHIFT_MASK),
                        VK_SHIFT_R => (_modifiers & ~SHIFT_MASK),
                        VK_CTRL_L => (_modifiers & ~CTRL_MASK),
                        VK_CTRL_R => (_modifiers & ~CTRL_MASK),
                        VK_ALT_L => (_modifiers & ~ALT_MASK),
                        VK_ALT_R => (_modifiers & ~ALT_MASK),
                        _ => _modifiers
                    };
                }
            }

            public int code => vkCode | modifiers;

            internal KeyboardEvent(KeyboardHookStructure structure, int modifiers)
            {
                _modifiers = modifiers;
                vkCode = structure.vkCode;
            }

            internal KeyboardEvent(int vkCode, int modifiers)
            {
                this.vkCode = vkCode;
                _modifiers = modifiers;
            }
        }
    }
}