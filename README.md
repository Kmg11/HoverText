# HoverTextWin — MVP

A Windows clone of macOS's Hover Text accessibility feature. Hold a modifier
key, point at any text on screen, and see it rendered large in a floating
overlay — using the real text content (via UI Automation), not a blurry
pixel zoom.

## How it works

1. `KeyboardHook.cs` installs a global low-level keyboard hook and fires
   `KeyDown`/`KeyUp` when **Left Ctrl** (the default trigger) is
   pressed/released.
2. While held, `App.xaml.cs` polls the cursor position every ~60ms
   (`GetCursorPos`).
3. `ElementTextExtractor.cs` asks Windows UI Automation (via the
   [FlaUI](https://github.com/FlaUI/FlaUI) wrapper) what element is at that
   point, and pulls its text — trying `TextPattern`, then `ValuePattern`,
   then the accessible `Name`, then `HelpText`, in that order.
4. `OverlayWindow.xaml` shows that text in a large, borderless, click-through,
   always-on-top popup near the cursor.
5. A tray icon lets you see it's running and exit.

This mirrors the actual mechanism macOS uses (AXUIElement → re-render as
text), rather than screen-zooming like Windows Magnifier does — so text
stays crisp at any size.

## Requirements

- Windows 10 or 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (recommended) or just the `dotnet` CLI
- Internet access to restore the FlaUI NuGet packages on first build

## Build & run

**Visual Studio:** open the folder, let it restore NuGet packages, press F5.

**CLI:**
```
dotnet restore
dotnet run
```

There's no visible main window — check the system tray for the icon once
it's running.

## Usage

1. Launch the app (tray icon appears).
2. Hold **Left Ctrl** and move your mouse over any text — button labels,
   paragraphs, tooltips, menu items, form fields.
3. Release Left Ctrl to hide the popup.
4. Right-click the tray icon → **Exit** to quit.

## Customizing

Everything tunable lives in `Config.cs`:
- `TriggerKey` — change to `0xA5` (Right Alt) or `0x14` (Caps Lock) etc.
- `FontSize`, `MaxWidth` — popup appearance
- `PollIntervalMs` — responsiveness vs. CPU usage
- `CursorGapY` / `CursorGapAboveY` — vertical gap between the popup and the pointer

Colors/border/shadow are in `OverlayWindow.xaml` if you want to restyle it.

## Known limitations (this is an MVP, not full parity)

- **Electron/Chromium apps** (Slack, Discord, VS Code, most browsers) often
  need accessibility support explicitly enabled by the app, and even then
  expose a thinner tree than native apps — expect inconsistent results.
- **Canvas-rendered UI** (games, some custom-drawn apps) exposes no
  accessible text at all via UIA. A real fix would add an OCR fallback
  (e.g. Windows' built-in `Windows.Media.Ocr`) when UIA comes back empty —
  not implemented here.
- **Elevated (Admin) apps**: Windows blocks non-elevated processes from
  inspecting elevated ones. If you need this to work over an app running
  as Administrator, run HoverTextWin as Administrator too.
- No triple-press "lock" mode like Mac's Hover Text (toggle without holding
  the key down) — straightforward to add to `KeyboardHook` if wanted.
- Font/weight/color of the *original* text isn't preserved — UIA gives you
  the string content, not its original styling, so everything renders in
  the overlay's own font (same tradeoff Mac's Hover Text makes, incidentally).
- No "Hover Color" equivalent (macOS's pointer-based color picker) — could
  be added by sampling the pixel under the cursor with `GetPixel`.

## A note on testing

This was written and reasoned through carefully, but built without access
to a Windows machine or NuGet in this environment, so it hasn't been
compiled here. The architecture and Win32/UIA usage are standard patterns,
but if you hit a compile error on first build (most likely spot: exact
FlaUI pattern-access syntax, which shifts slightly between major versions),
check the [FlaUI docs](https://github.com/FlaUI/FlaUI/wiki) — it's almost
always a one-line fix.
