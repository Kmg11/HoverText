using System;
using System.Windows;
using System.Windows.Controls;

namespace HoverTextWin
{
    /// <summary>
    /// Write-through options form: every control change is saved to the
    /// settings file and applied immediately, so there's no OK/Cancel.
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private readonly Settings _settings;
        private bool _initializing;

        public event Action? SettingsChanged;

        public OptionsWindow(Settings settings)
        {
            _settings = settings;
            _initializing = true;
            InitializeComponent();

            foreach (var (name, vk) in Settings.KnownTriggers)
                TriggerKeyCombo.Items.Add(new ComboBoxItem { Content = name, Tag = vk });
            TriggerKeyCombo.SelectedIndex = IndexOfTrigger(settings.TriggerKey);

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

        private int IndexOfTrigger(int vk)
        {
            for (int i = 0; i < Settings.KnownTriggers.Length; i++)
                if (Settings.KnownTriggers[i].Vk == vk) return i;
            return 0;
        }

        private void TriggerKeyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => SaveAndApply();

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

            if (TriggerKeyCombo.SelectedItem is ComboBoxItem item)
                _settings.TriggerKey = (int)item.Tag;
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
    }
}
