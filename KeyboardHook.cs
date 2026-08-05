using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HoverText
{
    /// <summary>
    /// Global low-level keyboard hook that raises KeyDown when every watched
    /// key is held simultaneously and KeyUp when any of them is released.
    /// This is how we detect the trigger modifier (or key chord) being held,
    /// mirroring how macOS Hover Text watches for Command.
    /// </summary>
    public class KeyboardHook
    {
        private readonly HashSet<int> _vkCodes;
        private readonly HashSet<int> _held = new();
        private readonly NativeMethods.LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;
        private bool _allDown;

        public event Action? KeyDown;
        public event Action? KeyUp;

        public KeyboardHook(IEnumerable<int> vkCodes)
        {
            _vkCodes = new HashSet<int>(vkCodes);
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
            _held.Clear();
            _allDown = false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

                if (_vkCodes.Contains(info.vkCode))
                {
                    int msg = wParam.ToInt32();
                    bool down = msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN;
                    bool up = msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP;

                    // _held.Add returns false on auto-repeat, so the chord
                    // transition fires only when the last required key goes down.
                    if (down && _held.Add(info.vkCode) && _held.Count == _vkCodes.Count && !_allDown)
                    {
                        _allDown = true;
                        KeyDown?.Invoke();
                    }
                    else if (up && _held.Remove(info.vkCode) && _allDown)
                    {
                        _allDown = false;
                        KeyUp?.Invoke();
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
