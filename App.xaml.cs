using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace HoverTextWin
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
        private bool _modifierHeld;
        private int _activeTriggerKey;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _settings = Settings.Load();
            _activeTriggerKey = _settings.TriggerKey;

            // Create the overlay and force its native window handle into
            // existence once (Show + immediate Hide), so click-through
            // styles are applied before it's ever actually shown to the user.
            _overlay = new OverlayWindow(_settings);
            _overlay.Show();
            _overlay.Hide();

            _hook = new KeyboardHook(_settings.TriggerKey);
            _hook.KeyDown += OnTriggerKeyDown;
            _hook.KeyUp += OnTriggerKeyUp;
            _hook.Start();

            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Config.PollIntervalMs)
            };
            _pollTimer.Tick += PollTimer_Tick;

            SetupTrayIcon();

            if (!_settings.HasSeenOnboarding)
                ShowOnboarding();
        }

        private void OnTriggerKeyDown()
        {
            _modifierHeld = true;
            _pollTimer!.Start();
        }

        private void OnTriggerKeyUp()
        {
            _modifierHeld = false;
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

        private void PollTimer_Tick(object? sender, EventArgs e)
        {
            if (!_modifierHeld) return;
            if (!NativeMethods.GetCursorPos(out var point)) return;

            var text = ElementTextExtractor.GetTextUnderPoint(point.X, point.Y);

            if (string.IsNullOrWhiteSpace(text))
            {
                _overlay!.HideOverlay();
                return;
            }

            _overlay!.ShowText(text, point.X, point.Y);
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
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

        private void OnSettingsChanged()
        {
            if (_hook != null && _settings!.TriggerKey != _activeTriggerKey)
            {
                _hook.Stop();
                _hook.KeyDown -= OnTriggerKeyDown;
                _hook.KeyUp -= OnTriggerKeyUp;

                _hook = new KeyboardHook(_settings.TriggerKey);
                _hook.KeyDown += OnTriggerKeyDown;
                _hook.KeyUp += OnTriggerKeyUp;
                _hook.Start();

                _activeTriggerKey = _settings.TriggerKey;
            }

            if (_trayIcon != null)
                _trayIcon.Text = $"Hover Text (hold {_settings!.TriggerKeyDisplayName})";
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Stop();
            _pollTimer?.Stop();

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
