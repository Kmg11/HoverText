# Contributing to Hover Text

Thanks for wanting to help! This is a small, Windows-only WPF app, and
every contribution is appreciated.

## Getting started

1. Fork the repo and clone your fork.
2. Requires .NET 10 SDK (`net10.0-windows` target).
3. Build: `dotnet build HoverText.slnx`
4. Run: `dotnet run` — there is no visible window; the app lives in the
   system tray. The only way out is tray icon → **Exit**.

There are no automated tests; verification is "it compiles and runs".

## Before you open a pull request

- Keep changes focused on one thing. If a fix is non-obvious, say why in
  the PR description.
- The app is Windows-only and built on Windows UI Automation (FlaUI).
  If you change behavior around UIA, note which apps you verified against.
- Follow the existing code style: single-project root files, tunables in
  `Config.cs`, all P/Invoke in `NativeMethods.cs`.
- Don't reformat code you aren't changing.

## Project layout

See `AGENTS.md` for a full architecture overview. Key files:

- `Config.cs` — all tunable constants.
- `KeyboardHook.cs` — global `WH_KEYBOARD_LL` hook.
- `ElementTextExtractor.cs` — UIA text extraction (never let UIA exceptions
  propagate to the UI thread).
- `OverlayWindow.xaml(.cs)` — the click-through, always-on-top popup.
- `NativeMethods.cs` — all P/Invoke.

## License

By contributing, you agree that your contributions are licensed under the
same [MIT License](LICENSE) as the project.
