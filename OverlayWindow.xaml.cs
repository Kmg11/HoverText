using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace HoverText
{
    public partial class OverlayWindow : Window
    {
        private readonly Settings _settings;
        private bool _clickThroughApplied;
        private string? _shownText;

        public OverlayWindow(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            SourceInitialized += (_, _) => MakeClickThrough();
        }

        /// <summary>The last text shown, used for copy-on-release.</summary>
        public string? LastShownText => _shownText;

        /// <summary>
        /// Applies WS_EX_TRANSPARENT so mouse clicks pass through the overlay
        /// to whatever app is underneath — the overlay is purely visual,
        /// same as Mac's Hover Text window.
        /// </summary>
        private void MakeClickThrough()
        {
            if (_clickThroughApplied) return;

            var hwnd = new WindowInteropHelper(this).Handle;
            var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
            var newStyle = exStyle
                            | NativeMethods.WS_EX_LAYERED
                            | NativeMethods.WS_EX_TRANSPARENT
                            | NativeMethods.WS_EX_TOOLWINDOW;

            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(newStyle));
            _clickThroughApplied = true;
        }

        public void ShowText(string text, int cursorX, int cursorY)
        {
            ApplyTheme();

            HoverTextBlock.FontSize = _settings.FontSize;
            HoverTextBlock.MaxWidth = _settings.MaxWidth;

            var textChanged = !string.Equals(text, _shownText, StringComparison.Ordinal);
            _shownText = text;
            HoverTextBlock.Text = text;
            Visibility = Visibility.Visible;

            // Keep the window anchored in place while the cursor moves around
            // within the same element; only re-position when the text under
            // the cursor actually changes (macOS Hover Text behaves the same
            // way — it doesn't chase the mouse on every move). Disabled from
            // options, the overlay always follows the cursor.
            if (_settings.AnchorPosition && !textChanged) return;

            // Force layout now so ActualWidth/Height reflect the new text
            // before we use them to position the window.
            UpdateLayout();

            var screen = SystemParameters.WorkArea;
            var cursor = ToDeviceIndependent(new System.Windows.Point(cursorX, cursorY));
            double left = cursor.X - ActualWidth / 2;
            double top = cursor.Y + _settings.CursorGapY;

            // Clamp horizontally so the centered window stays fully on-screen.
            if (left < screen.Left) left = screen.Left;
            else if (left + ActualWidth > screen.Right) left = screen.Right - ActualWidth;

            // Flip above the cursor if we'd run off the bottom of the screen.
            if (top + ActualHeight > screen.Bottom)
                top = cursor.Y - _settings.CursorGapAboveY - ActualHeight;
            if (top < screen.Top) top = screen.Top;

            Left = left;
            Top = top;
        }

        private void ApplyTheme()
        {
            if (_settings.UseLightTheme)
            {
                OverlayBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xEE, 0xFF, 0xFF, 0xFF));
                OverlayBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x55, 0x00, 0x00, 0x00));
                HoverTextBlock.Foreground = System.Windows.Media.Brushes.Black;
            }
            else
            {
                OverlayBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xEE, 0x1E, 0x1E, 0x1E));
                OverlayBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF));
                HoverTextBlock.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        // GetCursorPos returns physical pixels; WPF window positions are in
        // device-independent units. Convert so the overlay centers on the
        // cursor correctly regardless of display scaling.
        private System.Windows.Point ToDeviceIndependent(System.Windows.Point devicePoint)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
                return source.CompositionTarget.TransformFromDevice.Transform(devicePoint);
            return devicePoint;
        }
        public void HideOverlay()
        {
            if (Visibility != Visibility.Hidden)
                Visibility = Visibility.Hidden;
        }
    }
}
