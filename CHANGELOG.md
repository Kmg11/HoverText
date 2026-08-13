# Changelog

All notable changes to this project will be documented in this file.

## [0.0.3] - 2026-08-12

### Added

- Open-sourced the project: MIT License, contributing guide, code of
  conduct, and security policy.
- GitHub issue templates for bug reports and feature requests.
- Store-ready brand images: 2160×2160 box art and 1440×2160 poster.
- Assembly metadata (product, company, copyright, repository URL) in the
  project file.

### Changed

- Installer publisher now shows "Kirolos Mahfouz".

## [0.0.2] - 2026-08-09

### Added

- Brand-new landing page with an interactive demo: hold Left Ctrl and hover
  over the browser mockup to see the overlay work live.
- "About the creator" section with GitHub and LinkedIn links.

### Fixed

- Holding the trigger key no longer clashes with other shortcuts
  (Ctrl+C, Ctrl+Alt+Wheel) — the overlay backs off instead of flashing.
- Uninstall now fully removes the app folder and settings.

## [0.0.1] - 2026-08-08

### Added

- Initial release: hold a trigger key, point at text, and read it large in
  an always-on-top overlay.
- Text extraction via Windows UI Automation (TextPattern → ValuePattern →
  Name → HelpText).
- Options window: trigger keys, font size, max width, themes, anchoring,
  copy-on-release, launch at startup.
- System tray icon with Getting Started and Exit.
- Per-user Inno Setup installer with a "Launch with Windows" option.
