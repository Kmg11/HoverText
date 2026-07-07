using System.Drawing;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace HoverTextWin
{
    /// <summary>
    /// Wraps Windows UI Automation to pull the most useful text out of
    /// whatever element sits under a given screen point. This is the
    /// direct equivalent of what AXUIElement does on macOS for Hover Text.
    /// </summary>
    internal static class ElementTextExtractor
    {
        // One long-lived automation instance is much cheaper than creating
        // a new COM connection on every poll tick.
        private static readonly UIA3Automation Automation = new();

        public static string? GetTextUnderPoint(int x, int y)
        {
            try
            {
                var element = Automation.FromPoint(new Point(x, y));
                return ExtractText(element);
            }
            catch
            {
                // UIA throws for all sorts of transient reasons (element
                // torn down mid-call, provider not responding, elevated
                // process blocking access, etc). For a hover tool we just
                // skip that frame rather than crash the app.
                return null;
            }
        }

        private static string? ExtractText(AutomationElement? element)
        {
            if (element == null) return null;

            // 1) Rich text / documents: TextPattern gives the actual content,
            //    which is what you want for a text box, PDF viewer, editor, etc.
            try
            {
                if (element.Patterns.Text.IsSupported)
                {
                    var textPattern = element.Patterns.Text.Pattern;
                    var text = textPattern.DocumentRange.GetText(Config.MaxTextLength);
                    if (!string.IsNullOrWhiteSpace(text))
                        return Truncate(text.Trim());
                }
            }
            catch { /* fall through to next strategy */ }

            // 2) Editable fields / combo boxes: ValuePattern holds their content.
            try
            {
                if (element.Patterns.Value.IsSupported)
                {
                    var value = element.Patterns.Value.Pattern.Value.Value;
                    if (!string.IsNullOrWhiteSpace(value))
                        return Truncate(value.Trim());
                }
            }
            catch { }

            // 3) Everything else: buttons, labels, menu items, icons — the
            //    Name property is the accessible label most controls expose.
            try
            {
                if (!string.IsNullOrWhiteSpace(element.Name))
                    return Truncate(element.Name.Trim());
            }
            catch { }

            // 4) Last resort: tooltip/help text some controls provide.
            try
            {
                var helpText = element.Properties.HelpText.ValueOrDefault;
                if (!string.IsNullOrWhiteSpace(helpText))
                    return Truncate(helpText!.Trim());
            }
            catch { }

            return null;
        }

        private static string Truncate(string text) =>
            text.Length > Config.MaxTextLength ? text[..Config.MaxTextLength] + "…" : text;

        public static void Dispose() => Automation.Dispose();
    }
}
