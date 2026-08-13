using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HoverText
{
  /// <summary>
  /// Global low-level keyboard hook that raises KeyDown when every watched
  /// key is held simultaneously and KeyUp when any of them is released.
  /// This is how we detect the trigger modifier (or key combination) being
  /// held.
  /// </summary>
  public class KeyboardHook
  {
    private readonly HashSet<int> _vkCodes;
    private readonly HashSet<int> _held = new();
    private readonly HashSet<int> _otherHeld = new();
    private readonly NativeMethods.LowLevelKeyboardProc _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private bool _allDown;
    private bool _suppressed;

    public event Action? KeyDown;
    public event Action? KeyUp;

    /// <summary>
    /// Raised when a key outside the trigger chord is pressed while the
    /// chord is held — the chord is being used for another shortcut, so
    /// Hover Text must back off for this entire hold.
    /// </summary>
    public event Action? KeyCancelled;

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
      _otherHeld.Clear();
      _allDown = false;
      _suppressed = false;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0)
      {
        var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
        int vk = info.vkCode;
        int msg = wParam.ToInt32();
        bool down = msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN;
        bool up = msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP;

        if (down || up)
        {
          if (_vkCodes.Contains(vk))
          {
            // _held.Add returns false on auto-repeat, so the chord
            // transition fires only when the last required key goes down.
            if (down && _held.Add(vk) && _held.Count == _vkCodes.Count && !_allDown)
            {
              _allDown = true;

              // If an extra key was already held when the chord
              // completed (e.g. X pressed a moment before Ctrl),
              // this is a shortcut — don't engage Hover Text.
              if (_suppressed || _otherHeld.Count > 0)
                _suppressed = true;
              else
                KeyDown?.Invoke();
            }
            else if (up && _held.Remove(vk))
            {
              bool wasActive = _allDown;
              _allDown = false;
              if (wasActive)
                KeyUp?.Invoke();
              if (_held.Count == 0)
                _suppressed = false;
            }
          }
          else if (vk != NativeMethods.VK_PACKET)
          {
            if (down)
            {
              _otherHeld.Add(vk);

              // Chord is active and a non-trigger key went down:
              // the user is invoking another shortcut (Ctrl+C,
              // Ctrl+Alt+Wheel, ...). Cancel this hold.
              if (_allDown && !_suppressed)
              {
                _suppressed = true;
                _allDown = false;
                KeyCancelled?.Invoke();
              }
            }
            else if (up)
            {
              _otherHeld.Remove(vk);
            }
          }
        }
      }

      return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }
  }
}
