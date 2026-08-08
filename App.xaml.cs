using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace HoverText
{
    public partial class App : Application
    {
        private Settings? _settings;
        private NotifyIcon? _trayIcon;
        private OverlayWindow? _overlay;
        private OptionsWindow? _optionsWindow;
        private OnboardingWindow? _onboardingWindow;
        private KeyboardHook? _hook;
        private DispatcherTimer? _pollTimer;
        private DispatcherTimer? _activateTimer;
        private volatile bool _modifierHeld;
        private bool _pollInFlight;
        private int[] _activeTriggerKeys = Array.Empty<int>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _settings = Settings.Load();
            _activeTriggerKeys = _settings.TriggerKeys.ToArray();

            // Create the overlay and force its native window handle into
            // existence once (Show + immediate Hide), so click-through
            // styles are applied before it's ever actually shown to the user.
            _overlay = new OverlayWindow(_settings);
            _overlay.Show();
            _overlay.Hide();

            _hook = new KeyboardHook(_settings.TriggerKeys);
            _hook.KeyDown += OnTriggerKeyDown;
            _hook.KeyUp += OnTriggerKeyUp;
            _hook.KeyCancelled += OnTriggerKeyCancelled;
            _hook.Start();

            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Config.PollIntervalMs)
            };
            _pollTimer.Tick += PollTimer_Tick;

            // Activation delay: the chord must be held cleanly for a moment
            // before polling starts, so unrelated shortcuts (Ctrl+C, wheel)
            // don't flash the overlay.
            _activateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Config.ActivationDelayMs)
            };
            _activateTimer.Tick += ActivateTimer_Tick;

            SetupTrayIcon();

            if (!_settings.HasSeenOnboarding)
                ShowOnboarding();
        }

        private void OnTriggerKeyDown()
        {
            _modifierHeld = true;
            _activateTimer!.Start();
        }

        private void ActivateTimer_Tick(object? sender, EventArgs e)
        {
            _activateTimer!.Stop();
            if (_modifierHeld)
                _pollTimer!.Start();
        }

        private void OnTriggerKeyUp()
        {
            _modifierHeld = false;
            _activateTimer!.Stop();
            _pollTimer!.Stop();
            _overlay!.HideOverlay();

            if (_settings!.CopyOnRelease && !string.IsNullOrEmpty(_overlay.LastShownText))
            {
                try
                {
                    System.Windows.Clipboard.SetText(_overlay.LastShownText);
                }
                catch
                {
                    // Clipboard can be transiently locked by another app.
                }
            }
        }

        private void OnTriggerKeyCancelled()
        {
            // The chord was used for another shortcut (Ctrl+C, Ctrl+Alt+Wheel,
            // ...). Back off without touching the clipboard.
            _modifierHeld = false;
            _activateTimer!.Stop();
            _pollTimer!.Stop();
            _overlay!.HideOverlay();
        }

        private void PollTimer_Tick(object? sender, EventArgs e)
        {
            if (!_modifierHeld) return;
            if (_pollInFlight) return;

            // Safety net: if the KeyUp hook event was missed or delivered while
            // a slow UIA call had the UI thread busy, verify the trigger keys
            // are still physically held. Treat a release as a KeyUp so the
            // overlay never lingers after the button is let go.
            if (!AreTriggerKeysHeld())
            {
                OnTriggerKeyUp();
                return;
            }

            if (!NativeMethods.GetCursorPos(out var point)) return;

            _pollInFlight = true;
            var x = point.X;
            var y = point.Y;

            // UIA extraction can block for a long time (hung/unresponsive apps).
            // Run it off the UI thread so KeyUp is always processed promptly,
            // then marshal only the show/hide back onto the UI thread.
            Task.Run(() => ElementTextExtractor.GetTextUnderPoint(x, y))
                .ContinueWith(
                    t =>
                    {
                        _pollInFlight = false;
                        if (!_modifierHeld) return; // released mid-extraction
                        if (t.IsFaulted) return;

                        var text = t.Result;
                        if (string.IsNullOrWhiteSpace(text))
                            _overlay!.HideOverlay();
                        else
                            _overlay!.ShowText(text, x, y);
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool AreTriggerKeysHeld()
        {
            foreach (var vk in _activeTriggerKeys)
            {
                // High bit of GetAsyncKeyState is set while the key is down.
                if ((NativeMethods.GetAsyncKeyState(vk) & 0x8000) == 0)
                    return false;
            }
            return true;
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = LoadAppIcon(),
                Visible = true,
                Text = $"Hover Text (hold {_settings!.TriggerKeyDisplayName})"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Options...", null, (_, _) => ShowOptionsWindow());
            menu.Items.Add("Getting Started", null, (_, _) => ShowOnboarding());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (_, _) => Shutdown());
            _trayIcon.ContextMenuStrip = menu;
        }

        private void ShowOnboarding()
        {
            if (_onboardingWindow == null)
            {
                _onboardingWindow = new OnboardingWindow(_settings!);
                _onboardingWindow.Completed += () =>
                {
                    _settings!.HasSeenOnboarding = true;
                    _settings.Save();
                };
                _onboardingWindow.Closed += (_, _) => _onboardingWindow = null;
            }

            _onboardingWindow.Show();
            _onboardingWindow.Activate();
        }

        private void ShowOptionsWindow()
        {
            if (_optionsWindow == null)
            {
                _optionsWindow = new OptionsWindow(_settings!);
                _optionsWindow.SettingsChanged += OnSettingsChanged;
                _optionsWindow.Closed += (_, _) => _optionsWindow = null;
            }

            _optionsWindow.Show();
            _optionsWindow.Activate();
        }

        /// <summary>
        /// Loads the embedded app icon for the tray. Falls back to the default
        /// application icon if the resource is somehow missing.
        /// </summary>
        private static System.Drawing.Icon LoadAppIcon()
        {
            try
            {
                var info = System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/assets/app.ico"));
                if (info != null) return new System.Drawing.Icon(info.Stream);
            }
            catch
            {
                // Fall through to the generic icon.
            }

            return SystemIcons.Application;
        }

        private void OnSettingsChanged()
        {
            if (_hook != null && !_settings!.TriggerKeys.SequenceEqual(_activeTriggerKeys))
            {
                _hook.Stop();
                _hook.KeyDown -= OnTriggerKeyDown;
                _hook.KeyUp -= OnTriggerKeyUp;
                _hook.KeyCancelled -= OnTriggerKeyCancelled;

                _hook = new KeyboardHook(_settings.TriggerKeys);
                _hook.KeyDown += OnTriggerKeyDown;
                _hook.KeyUp += OnTriggerKeyUp;
                _hook.KeyCancelled += OnTriggerKeyCancelled;
                _hook.Start();

                _activeTriggerKeys = _settings.TriggerKeys.ToArray();
            }

            if (_trayIcon != null)
                _trayIcon.Text = $"Hover Text (hold {_settings!.TriggerKeyDisplayName})";
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Stop();
            _pollTimer?.Stop();
            _activateTimer?.Stop();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }

            ElementTextExtractor.Dispose();
            base.OnExit(e);
        }
    }
}
