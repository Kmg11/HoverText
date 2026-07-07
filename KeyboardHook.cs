using System;
using System.Runtime.InteropServices;

namespace HoverTextWin
{
    /// <summary>
    /// Global low-level keyboard hook that raises KeyDown/KeyUp for a single
    /// virtual key code. This is how we detect the trigger modifier being
    /// held down, mirroring how macOS Hover Text watches for Command.
    /// </summary>
    public class KeyboardHook
    {
        private readonly int _vkCode;
        private readonly NativeMethods.LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;
        private bool _isDown;

        public event Action? KeyDown;
        public event Action? KeyUp;

        public KeyboardHook(int vkCode)
        {
            _vkCode = vkCode;
            // Keep a reference to the delegate for the lifetime of the hook.
            // If this gets garbage collected, the native callback pointer
            // becomes invalid and the hook silently stops working (or crashes).
            _proc = HookCallback;
        }

        public void Start()
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            _hookId = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _proc,
                NativeMethods.GetModuleHandle(curModule!.ModuleName!),
                0);

            if (_hookId == IntPtr.Zero)
            {
                var err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to install keyboard hook (Win32 error {err}).");
            }
        }

        public void Stop()
        {
            if (_hookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

                if (info.vkCode == _vkCode)
                {
                    int msg = wParam.ToInt32();

                    if ((msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN) && !_isDown)
                    {
                        _isDown = true;
                        KeyDown?.Invoke();
                    }
                    else if (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP)
                    {
                        _isDown = false;
                        KeyUp?.Invoke();
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
