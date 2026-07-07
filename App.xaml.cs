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
        private NotifyIcon? _trayIcon;
        private OverlayWindow? _overlay;
        private KeyboardHook? _hook;
        private DispatcherTimer? _pollTimer;
        private bool _modifierHeld;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create the overlay and force its native window handle into
            // existence once (Show + immediate Hide), so click-through
            // styles are applied before it's ever actually shown to the user.
            _overlay = new OverlayWindow();
            _overlay.Show();
            _overlay.Hide();

            _hook = new KeyboardHook(Config.TriggerKey);
            _hook.KeyDown += OnTriggerKeyDown;
            _hook.KeyUp += OnTriggerKeyUp;
            _hook.Start();

            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Config.PollIntervalMs)
            };
            _pollTimer.Tick += PollTimer_Tick;

            SetupTrayIcon();
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
                Text = $"Hover Text (hold {Config.TriggerKeyDisplayName})"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add($"Hold {Config.TriggerKeyDisplayName} + hover to zoom text", null, (_, _) => { }).Enabled = false;
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (_, _) => Shutdown());
            _trayIcon.ContextMenuStrip = menu;
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
