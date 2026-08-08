namespace HoverText
{
    /// <summary>
    /// All the tunable knobs for the MVP live here so you don't have to
    /// hunt through the logic files to tweak behavior.
    /// </summary>
    public static class Config
    {
        // Fallback default trigger (0xA2 = VK_LCONTROL = Left Ctrl); only used
        // when no settings file exists. Users can pick any key or chord via
        // Options → Trigger keys; this is just the starting value.
        // Common alternatives: 0xA5 = VK_RMENU (Right Alt), 0x14 = VK_CAPITAL (Caps Lock).
        public const int TriggerKey = 0xA2;

        // How often (ms) to re-check the element under the cursor while the
        // trigger key is held. Lower = more responsive, higher = less CPU.
        public const int PollIntervalMs = 60;

        // The trigger chord must be held cleanly this long (ms) before Hover
        // Text engages. Gives shortcuts like Ctrl+C / Ctrl+Alt+Wheel time to
        // be recognized so the overlay doesn't flash for unrelated holds.
        public const int ActivationDelayMs = 100;

        // Overlay appearance
        public const double FontSize = 32;
        public const double MaxWidth = 700;

        // The overlay is centered horizontally below the cursor, this far down
        // (px). Keeps the popup clear of the pointer so it doesn't obscure the
        // text being read.
        public const int CursorGapY = 24;

        // When there's no room below the cursor, the overlay flips above it
        // with this gap (px).
        public const int CursorGapAboveY = 24;

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
