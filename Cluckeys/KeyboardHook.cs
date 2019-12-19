using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Cluckeys
{
    /// <summary>
    /// Keyboard hook.
    /// 
    /// Docs: https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/
    /// </summary>
    public class KeyboardHook
    {
        // Installs a hook procedure that monitors low-level keyboard input events.
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100; // KEYDOWN
        private const int WM_KEYUP = 0x101; // KEYUP
        private const int WM_SYSKEYDOWN = 0x104; // SYSKEYDOWN
        private const int WM_SYSKEYUP = 0x105; // SYSKEYUP

        // The keyboard hook handle.
        private int _keyboardHookHandle;

        private readonly Dictionary<int, int> _eventDict = new Dictionary<int, int>();

        private OnKeyEvent _onKeyDownEvent;
        private OnKeyEvent _onKeyTypeEvent;
        private OnKeyEvent _onKeyUpEvent;

        public OnKeyEvent OnKeyDownEvent
        {
            set => _onKeyDownEvent = value;
        }

        public OnKeyEvent OnKeyTypeEvent
        {
            set => _onKeyTypeEvent = value;
        }

        public OnKeyEvent OnKeyUpEvent
        {
            set => _onKeyUpEvent = value;
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
        /// <param name="lpfn"></param>
        /// <param name="hInstance"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);


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

        public void Start()
        {
            if (_keyboardHookHandle != 0) return;

            // 安装键盘钩子 
            var moduleName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.ModuleName;
            var moduleHandle = GetModuleHandle(moduleName);
            _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, moduleHandle, 0);

            if (_keyboardHookHandle == 0)
            {
                throw new Exception("Keyboard hook installation failed");
            }
        }

        private int LowLevelKeyboardProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var keyboardEvent = (KeyboardEvent) Marshal.PtrToStructure(lParam, typeof(KeyboardEvent));
                switch (wParam)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                    {
                        var count = _eventDict.GetValueOrDefault(keyboardEvent.vkCode, 0) + 1;
                        _eventDict[keyboardEvent.vkCode] = count;
                        var fun = count == 1 ? _onKeyDownEvent : _onKeyTypeEvent;
                        fun?.Invoke(keyboardEvent);
                        break;
                    }
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        _eventDict.Remove(keyboardEvent.vkCode);
                        _onKeyUpEvent?.Invoke(keyboardEvent);
                        break;
                }
            }

            // 如果返回1，则结束消息，这个消息到此为止，不再传递。
            // 如果返回0或调用CallNextHookEx函数则消息出了这个钩子继续往下传递，也就是传给消息真正的接受者。
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        public void Stop()
        {
            if (_keyboardHookHandle == 0) return;
            if (UnhookWindowsHookEx(_keyboardHookHandle))
            {
                throw new Exception("Unable to unhook keyboard hook: " + _keyboardHookHandle);
            }

            _keyboardHookHandle = 0;
        }

        ~KeyboardHook()
        {
            Stop();
        }


        /// <summary>
        /// 键盘事件结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardEvent
        {
            /// <summary>
            /// 虚拟键码，其值的范围是1至254
            /// </summary>
            public readonly int vkCode;

            /// <summary>
            /// 硬件扫描码
            /// </summary>
            public readonly int scanCode;

            /// <summary>
            /// 键标志
            /// </summary>
            public readonly int flags;

            /// <summary>
            /// 时间戳
            /// </summary>
            public readonly int time;

            /// <summary>
            /// 额外信息
            /// </summary>
            public readonly int dwExtraInfo;
        }
    }
}