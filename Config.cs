namespace HoverTextWin
{
    /// <summary>
    /// All the tunable knobs for the MVP live here so you don't have to
    /// hunt through the logic files to tweak behavior.
    /// </summary>
    public static class Config
    {
        // Virtual key code for the trigger/modifier key. Default: Left Ctrl.
        // Common alternatives: 0xA5 = VK_RMENU (Right Alt), 0x14 = VK_CAPITAL (Caps Lock).
        public const int TriggerKey = 0xA2; // VK_RCONTROL
        public const string TriggerKeyDisplayName = "Left Ctrl";

        // How often (ms) to re-check the element under the cursor while the
        // trigger key is held. Lower = more responsive, higher = less CPU.
        public const int PollIntervalMs = 60;

        // Overlay appearance
        public const double FontSize = 32;
        public const double MaxWidth = 700;

        // Offset (px) from the actual cursor position so the popup doesn't
        // sit directly under the mouse and obscure what you're pointing at.
        public const int CursorOffsetX = 24;
        public const int CursorOffsetY = 24;

        // Safety cap so a giant document element doesn't produce a wall of text.
        public const int MaxTextLength = 1500;

        // How many levels up to search for a TextPattern host when reading a
        // hyperlink (browsers expose the URL as the link's Name, not the text).
        public const int MaxHyperlinkAncestorDepth = 10;

        // A Name matching this pattern is treated as a link/URL rather than
        // display text (e.g. a browser exposed the href as the element's Name).
        public const string UrlNameRegex = @"^(https?://|www\.)\S+$";
    }
}
