using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;

namespace HoverText
{
    /// <summary>
    /// User-editable settings, persisted as JSON under
    /// %LocalAppData%\HoverText\settings.json. Config.cs remains the source
    /// of default values; this class falls back to them for anything the
    /// settings file doesn't contain.
    /// </summary>
    public sealed class Settings
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "HoverText";

        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HoverText");

        private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

        /// <summary>
        /// Virtual-key codes that must be held together to activate Hover Text.
        /// One key is the common case; a chord like Ctrl+Shift also works.
        /// </summary>
        public List<int> TriggerKeys { get; set; } = new() { Config.TriggerKey };

        public double FontSize { get; set; } = Config.FontSize;
        public double MaxWidth { get; set; } = Config.MaxWidth;
        public int CursorGapY { get; set; } = Config.CursorGapY;
        public int CursorGapAboveY { get; set; } = Config.CursorGapAboveY;

        /// <summary>
        /// When true, the overlay stays put while the text under the cursor is
        /// unchanged; when false it always re-centers under the cursor.
        /// </summary>
        public bool AnchorPosition { get; set; } = true;

        public bool LaunchAtStartup { get; set; }
        public bool CopyOnRelease { get; set; }
        public bool UseLightTheme { get; set; }

        /// <summary>True once the first-launch onboarding screen has been dismissed.</summary>
        public bool HasSeenOnboarding { get; set; }

        [JsonIgnore]
        public string TriggerKeyDisplayName =>
            string.Join(" + ", TriggerKeys.Select(NativeMethods.KeyName).Distinct());

        /// <summary>
        /// Ensures the trigger is never empty or duplicated (e.g. after
        /// loading a hand-edited or older settings file).
        /// </summary>
        public void Normalize()
        {
            if (TriggerKeys == null || TriggerKeys.Count == 0)
                TriggerKeys = new List<int> { Config.TriggerKey };
            TriggerKeys = TriggerKeys.Distinct().ToList();
        }

        public static Settings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return new Settings();
                var json = File.ReadAllText(SettingsPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var settings = JsonSerializer.Deserialize<Settings>(json, options) ?? new Settings();
                settings.Normalize();
                return settings;
            }
            catch
            {
                // Unreadable settings never prevent the app from starting.
                return new Settings();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
            ApplyLaunchAtStartup();
        }

        private void ApplyLaunchAtStartup()
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (LaunchAtStartup)
                key?.SetValue(RunValueName, $"\"{Environment.ProcessPath}\"");
            else
                key?.DeleteValue(RunValueName, throwOnMissingValue: false);
        }
    }
}
