using System;
using System.Windows;
using System.Windows.Interop;

namespace HoverTextWin
{
    public partial class OverlayWindow : Window
    {
        private bool _clickThroughApplied;

        public OverlayWindow()
        {
            InitializeComponent();
            HoverTextBlock.FontSize = Config.FontSize;
            HoverTextBlock.MaxWidth = Config.MaxWidth;
            SourceInitialized += (_, _) => MakeClickThrough();
        }

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
            HoverTextBlock.Text = text;
            Visibility = Visibility.Visible;

            // Force layout now so ActualWidth/Height reflect the new text
            // before we use them to position the window.
            UpdateLayout();

            var screen = SystemParameters.WorkArea;
            double left = cursorX + Config.CursorOffsetX;
            double top = cursorY + Config.CursorOffsetY;

            // Flip to the other side of the cursor if we'd run off-screen.
            if (left + ActualWidth > screen.Right)
                left = cursorX - Config.CursorOffsetX - ActualWidth;
            if (top + ActualHeight > screen.Bottom)
                top = cursorY - Config.CursorOffsetY - ActualHeight;

            Left = Math.Max(screen.Left, left);
            Top = Math.Max(screen.Top, top);
        }

        public void HideOverlay()
        {
            if (Visibility != Visibility.Hidden)
                Visibility = Visibility.Hidden;
        }
    }
}
