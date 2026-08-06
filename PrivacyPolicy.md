# Privacy Policy — Hover Text

**Effective date:** [Date]

## Overview

Hover Text ("the App", "we", "us") is a Windows desktop accessibility utility that displays on-screen text in a large, readable popup. This Privacy Policy explains how the App handles your information. In short: **Hover Text does not collect, store, or transmit any personal data.** It is a fully local, offline application.

## Data Collection & Processing

### On-screen text
The App uses Windows UI Automation (via the FlaUI library) to read the text content of the element under the mouse cursor so it can display that text in an overlay window. This inspection happens **entirely in memory on your device**. Screen text is never logged, written to disk, or sent to any server.

### Keyboard input
The App installs a low-level keyboard hook solely to detect when the user-configured trigger key or chord is being held (e.g., holding Left Ctrl to activate the overlay). Keyboard events are examined only to match the trigger, are never recorded, and are discarded immediately. No keystroke content is logged, stored, or transmitted.

## Local Storage

The App stores your preferences locally on your machine in a single settings file:

- **Location:** `%LocalAppData%\HoverText\settings.json`
- **Contents:** custom trigger keys, overlay font size, max width, cursor-positioning gap, theme preference, clipboard-copy option, and launch-at-startup flag.

If you enable **Launch at startup**, the App registers a standard Windows Run registry value pointing to its own executable. This is a local registry entry on your device.

These settings never leave your device and are used only to configure the App's behavior.

## Clipboard

If you enable the optional **"Copy hovered text to clipboard on release"** setting, the App writes the text currently displayed in the overlay to your Windows clipboard when you release the trigger key. This is a user-initiated, opt-in action using your device's standard clipboard — exactly as if you pressed Ctrl+C. The App does not read your clipboard history or any other clipboard contents.

## Third-Party Sharing

The App does **not** share, sell, rent, or disclose any user data to any third party. It contains no third-party analytics, advertising, or tracking SDKs.

## Internet & Data Transmission

The App makes **no network connections** and performs **zero data transmission**. It:

- does not require or use an internet connection;
- does not send telemetry, diagnostics, or crash reports;
- contains no advertising or tracking;
- collects no personal information of any kind.

The only software dependencies are the FlaUI UI Automation libraries, which run locally and do not communicate over the network.

## Data Retention & Deletion

Because the App does not collect any personal data, there is nothing to retain or delete. Removing the App uninstalls its program files; you may additionally delete the `%LocalAppData%\HoverText` folder to remove your local preferences.

## Children's Privacy

The App does not collect any personal information from anyone, including children under the age of 13.

## Changes to This Policy

We may update this Privacy Policy from time to time. Any changes will be reflected by updating the "Effective date" above. Since the App performs no data collection, material changes are unlikely.

## Contact Information

If you have any questions about this Privacy Policy, you may contact us at:

- Developer: **Kirolos Mahfouz**
- Email: **kirolosmahfouz15@gmail.com**
- Website: (coming soon)

## Final Example

Privacy Policy — Hover Text

Effective date: August 6, 2026

Overview

Hover Text ("the App", "we", "us") is a Windows desktop accessibility utility that displays on-screen text in a large, readable popup. This Privacy Policy explains how the App handles your information. In short: Hover Text does not collect, store, or transmit any personal data. It is a fully local, offline application.

Data Collection & Processing

On-screen text
The App uses Windows UI Automation (via the FlaUI library) to read the text content of the element under the mouse cursor so it can display that text in an overlay window. This inspection happens entirely in memory on your device. Screen text is never logged, written to disk, or sent to any server.

Keyboard input
The App installs a low-level keyboard hook solely to detect when the user-configured trigger key or chord is being held (e.g., holding Left Ctrl to activate the overlay). Keyboard events are examined only to match the trigger, are never recorded, and are discarded immediately. No keystroke content is logged, stored, or transmitted.

Local Storage

The App stores your preferences locally on your machine in a single settings file:

Location: %LocalAppData%\HoverText\settings.json

Contents: Custom trigger keys, overlay font size, max width, cursor-positioning gap, theme preference, clipboard-copy option, and launch-at-startup flag.

If you enable Launch at startup, the App registers a standard Windows Run registry value pointing to its own executable. This is a local registry entry on your device.

These settings never leave your device and are used only to configure the App's behavior.

Clipboard

If you enable the optional "Copy hovered text to clipboard on release" setting, the App writes the text currently displayed in the overlay to your Windows clipboard when you release the trigger key. This is a user-initiated, opt-in action using your device's standard clipboard — exactly as if you pressed Ctrl+C. The App does not read your clipboard history or any other clipboard contents.

Third-Party Sharing

The App does not share, sell, rent, or disclose any user data to any third party. It contains no third-party analytics, advertising, or tracking SDKs.

Internet & Data Transmission
The App makes no network connections and performs zero data transmission. It:

does not require or use an internet connection;

does not send telemetry, diagnostics, or crash reports;

contains no advertising or tracking;

collects no personal information of any kind.

The only software dependencies are the FlaUI UI Automation libraries, which run locally and do not communicate over the network.

Data Retention & Deletion
Because the App does not collect any personal data, there is nothing to retain or delete. Removing the App uninstalls its program files; you may additionally delete the %LocalAppData%\HoverText folder to remove your local preferences.

Children's Privacy
The App does not collect any personal information from anyone, including children under the age of 13.

Changes to This Policy
We may update this Privacy Policy from time to time. Any changes will be reflected by updating the "Effective date" above. Since the App performs no data collection, material changes are unlikely.

Contact Information
If you have any questions about this Privacy Policy, you may contact us at:

Developer: Kirolos Mahfouz
Email: kirolosmahfouz15@gmail.com
