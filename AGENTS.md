# AGENTS.md

WPF desktop app that clones macOS Hover Text: hold a modifier key, point at
text, and a large always-on-top overlay shows the real text under the cursor
(via Windows UI Automation/FlaUI, not a pixel zoom). Single-project .NET app,
Windows-only, no tests, no CI, no lint config.

## Build & verify

- Requires .NET 10 SDK (`net10.0-windows` target). The README's ".NET 8"
  claim is stale — the csproj is the source of truth.
- Build: `dotnet build HoverText.slnx`
- Run: `dotnet run` (or build + run `bin/Debug/net10.0-windows/HoverText.exe`)
- There is no visible window. The app lives in the system tray; the only way
  out is tray icon → **Exit**. `App.xaml` uses `ShutdownMode="OnExplicitShutdown"`.
- Verification of any change = it compiles and runs; there are no tests.

## Architecture

All files are in the project root:

- `Config.cs` — every tunable constant (default trigger key, poll interval,
  overlay size, offsets, max text length). Change knobs here, not in the logic
  files.
- `KeyboardHook.cs` — global low-level keyboard hook (`WH_KEYBOARD_LL`) that
  raises `KeyDown` when every watched VK code is held simultaneously and
  `KeyUp` when any of them is released (supports a single key or a chord).
- `App.xaml.cs` — wires everything; a `DispatcherTimer` polls the cursor
  position every `PollIntervalMs` while the trigger keys are held.
- `ElementTextExtractor.cs` — long-lived `UIA3Automation` singleton; pulls
  text under a point, trying TextPattern → ValuePattern → Name → HelpText.
- `OverlayWindow.xaml(.cs)` — borderless, topmost, click-through popup
  (`WS_EX_TRANSPARENT` + `WS_EX_NOACTIVATE`, so it never steals focus).
  Styling lives here.
- `NativeMethods.cs` — all P/Invoke, incl. 32/64-bit-safe
  `GetWindowLongPtr`/`SetWindowLongPtr` wrappers and `KeyName()` for friendly
  key names.
- WinForms is referenced only for the tray `NotifyIcon`; `UseWindowsForms`
  in the csproj is load-bearing.

## Gotchas

- The default trigger is **Left Ctrl** (`Config.TriggerKey = 0xA2` =
  `VK_LCONTROL`), but `Config.cs` comments and the README say "Left Ctrl".
  That documentation is wrong; don't "fix" the constant to match it. Note
  the user can now pick any key or chord via Options → Trigger keys; the
  constant is only the fallback default in `Settings.TriggerKeys`.
- `OptionsWindow` records the trigger via a *second*, temporary
  `WH_KEYBOARD_LL` hook (`OptionsWindow.xaml.cs`). Enter finishes, Esc
  cancels; those VK codes are never captured. The recording hook and the
  main app hook can coexist.
- `KeyboardHook` deliberately keeps a strong reference to the hook callback
  delegate so it isn't garbage-collected out from under the native hook.
  Preserve that if you refactor.
- `App.xaml.cs` shows + immediately hides `OverlayWindow` at startup purely
  to force its HWND into existence so the click-through exstyle is applied
  before first display. Don't remove it as "dead code".
- `ElementTextExtractor` swallows all UIA exceptions and returns `null`;
  transient failures silently skip that frame. Keep that contract — never
  let UIA exceptions propagate to the UI thread.
- Non-elevated processes cannot read text from elevated (admin) apps; the
  app must also run as admin to inspect them.
- `obj/` contains stale artifacts (`Themes/Theme.xaml`, `OptionsWindow`) that
  no longer exist in source. Ignore them; only build outputs from the current
  file set matter. `bin/`, `obj/`, `.vs/` are gitignored.
