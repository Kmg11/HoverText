using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HoverText
{
    /// <summary>
    /// Write-through options form: every control change is saved to the
    /// settings file and applied immediately, so there's no OK/Cancel. The
    /// trigger keys are captured by pressing them while recording.
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private readonly Settings _settings;
        private bool _initializing;
        private bool _recording;
        private readonly HashSet<int> _captured = new();
        private NativeMethods.LowLevelKeyboardProc? _recordProc;
        private IntPtr _recordHook = IntPtr.Zero;

        public event Action? SettingsChanged;

        public OptionsWindow(Settings settings)
        {
            _settings = settings;
            _initializing = true;
            InitializeComponent();

            TriggerDisplay.Text = settings.TriggerKeyDisplayName;

            FontSizeSlider.Value = settings.FontSize;
            MaxWidthSlider.Value = settings.MaxWidth;
            GapSlider.Value = settings.CursorGapY;

            DarkThemeRadio.IsChecked = !settings.UseLightTheme;
            LightThemeRadio.IsChecked = settings.UseLightTheme;
            AnchorCheckBox.IsChecked = settings.AnchorPosition;
            CopyCheckBox.IsChecked = settings.CopyOnRelease;
            StartupCheckBox.IsChecked = settings.LaunchAtStartup;

            _initializing = false;
        }

        private void ChangeTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (!_recording) StartRecording();
            else FinishRecording();
        }

        private void CancelTrigger_Click(object sender, RoutedEventArgs e)
            => CancelRecording();

        private void StartRecording()
        {
            _captured.Clear();
            _recording = true;
            ChangeTriggerButton.Content = "Done";
            CancelTriggerButton.Visibility = Visibility.Visible;
            TriggerHint.Text = "Press the key or keys to use as the trigger, then click Done (Esc cancels).";
            Keyboard.ClearFocus();

            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            _recordProc = RecordCallback;
            _recordHook = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _recordProc,
                NativeMethods.GetModuleHandle(curModule!.ModuleName!),
                0);

            if (_recordHook == IntPtr.Zero)
            {
                _recording = false;
                CancelRecording();
            }
        }

        private IntPtr RecordCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN)
                {
                    var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                    int vk = info.vkCode;

                    if (vk == NativeMethods.VK_RETURN)
                        Dispatcher.BeginInvoke(FinishRecording);
                    else if (vk == NativeMethods.VK_ESCAPE)
                        Dispatcher.BeginInvoke(CancelRecording);
                    else if (vk != NativeMethods.VK_PACKET && _captured.Add(vk))
                        Dispatcher.BeginInvoke(() => UpdateTriggerDisplay());
                }
            }

            return NativeMethods.CallNextHookEx(_recordHook, nCode, wParam, lParam);
        }

        private void FinishRecording()
        {
            if (!_recording) return;
            StopRecording();

            if (_captured.Count == 0)
            {
                ResetTriggerUi();
                return;
            }

            _settings.TriggerKeys = _captured.ToList();
            _settings.Save();
            SettingsChanged?.Invoke();
            ResetTriggerUi();
        }

        private void CancelRecording()
        {
            if (_recording) StopRecording();
            ResetTriggerUi();
        }

        private void StopRecording()
        {
            if (_recordHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_recordHook);
                _recordHook = IntPtr.Zero;
            }
            _recordProc = null;
            _recording = false;
        }

        private void ResetTriggerUi()
        {
            ChangeTriggerButton.Content = "Change...";
            CancelTriggerButton.Visibility = Visibility.Collapsed;
            TriggerDisplay.Text = _settings.TriggerKeyDisplayName;
            TriggerHint.Text = "Press the Change button, then press the key or keys to use, then click Done.";
        }

        private void UpdateTriggerDisplay()
        {
            TriggerDisplay.Text = string.Join(" + ", _captured.Select(NativeMethods.KeyName));
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FontSizeValueLabel.Text = $"{e.NewValue:0} px";
            SaveAndApply();
        }

        private void MaxWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MaxWidthValueLabel.Text = $"{e.NewValue:0} px";
            SaveAndApply();
        }

        private void GapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GapValueLabel.Text = $"{e.NewValue:0} px";
            SaveAndApply();
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
            => SaveAndApply();

        private void BehaviorCheckBox_Checked(object sender, RoutedEventArgs e)
            => SaveAndApply();

        private void Close_Click(object sender, RoutedEventArgs e)
            => Close();

        private void SaveAndApply()
        {
            if (_initializing) return;

            _settings.FontSize = Math.Round(FontSizeSlider.Value, 0);
            _settings.MaxWidth = Math.Round(MaxWidthSlider.Value, 0);
            _settings.CursorGapY = (int)Math.Round(GapSlider.Value, 0);
            _settings.UseLightTheme = LightThemeRadio.IsChecked == true;
            _settings.AnchorPosition = AnchorCheckBox.IsChecked == true;
            _settings.CopyOnRelease = CopyCheckBox.IsChecked == true;
            _settings.LaunchAtStartup = StartupCheckBox.IsChecked == true;

            _settings.Save();
            SettingsChanged?.Invoke();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_recording) StopRecording();
            base.OnClosed(e);
        }
    }
}
