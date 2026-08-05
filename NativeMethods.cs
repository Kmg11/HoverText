using System;
using System.Runtime.InteropServices;

namespace HoverText
{
    internal static class NativeMethods
    {
        // ---------- Cursor position ----------

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        // ---------- Low-level keyboard hook ----------

        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        // ---------- Key names / capture ----------

        // Reserved while recording a trigger: Enter finishes, Esc cancels.
        public const int VK_RETURN = 0x0D;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_PACKET = 0xE7;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetKeyNameText(IntPtr lParam, System.Text.StringBuilder lpString, int cchSize);

        /// <summary>
        /// Friendly, localized name for a virtual key code (e.g. "Ctrl",
        /// "A", "Page Up"). Modifier keys get clean names regardless of side.
        /// </summary>
        public static string KeyName(int vkCode)
        {
            return vkCode switch
            {
                0xA0 or 0xA1 => "Shift",
                0xA2 or 0xA3 => "Ctrl",
                0xA4 or 0xA5 => "Alt",
                0x5B or 0x5C => "Win",
                _ => MapKeyName(vkCode)
            };
        }

        private static string MapKeyName(int vkCode)
        {
            uint scan = MapVirtualKey((uint)vkCode, 0);
            // lParam layout: scan code in bits 16-23, extended flag in bit 24.
            var lParam = new IntPtr((long)(scan << 16) | 0x01000000);
            var sb = new System.Text.StringBuilder(64);
            if (GetKeyNameText(lParam, sb, sb.Capacity) > 0)
                return sb.ToString();
            return $"VK {vkCode}";
        }

        // ---------- Click-through / tool-window styles for the overlay ----------

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // 32/64-bit safe wrappers (GetWindowLongPtr only exists on 64-bit user32).
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, nIndex)
                : new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }
    }
}
