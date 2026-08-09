using System.Drawing;
using System.Text.RegularExpressions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace HoverText
{
    /// <summary>
    /// Wraps Windows UI Automation to pull the most useful text out of
    /// whatever element sits under a given screen point. Re-renders the
    /// accessible text content rather than zooming the screen.
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
                return ExtractText(element, new Point(x, y));
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

        private static string? ExtractText(AutomationElement? element, Point point)
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

            // 2) Hyperlinks: browsers expose the URL as the link's Name AND as
            //    the ValuePattern's value, so both are useless to show. Prefer
            //    the visible link text, in order of reliability:
            //    a) a Text element nested inside the link (Chrome/Edge/Firefox
            //       expose the rendered link text as a child Text node),
            //    b) the TextPattern range covering the link from the enclosing
            //       document,
            //    c) the link's own Name as a last resort.
            try
            {
                var name = element.Name;
                if (element.ControlType == ControlType.Hyperlink || LooksLikeUrl(name))
                {
                    var linkText = GetLinkText(element, point);
                    if (!string.IsNullOrWhiteSpace(linkText))
                        return Truncate(linkText.Trim());
                    var childText = GetDescendantText(element);
                    if (!string.IsNullOrWhiteSpace(childText))
                        return Truncate(childText.Trim());
                    if (!string.IsNullOrWhiteSpace(name))
                        return Truncate(name.Trim());
                }
            }
            catch { /* fall through to next strategy */ }

            // 3) Editable fields / combo boxes: ValuePattern holds their content.
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

            // 4) Everything else: buttons, labels, menu items, icons — the
            //    Name property is the accessible label most controls expose.
            try
            {
                if (!string.IsNullOrWhiteSpace(element.Name))
                    return Truncate(element.Name.Trim());
            }
            catch { }

            // 5) Last resort: tooltip/help text some controls provide.
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

        private static string? GetLinkText(AutomationElement link, Point point)
        {
            var current = link;
            for (var depth = 0; depth <= Config.MaxHyperlinkAncestorDepth && current != null; depth++)
            {
                if (current.Patterns.Text.IsSupported)
                {
                    try
                    {
                        var textPattern = current.Patterns.Text.Pattern;
                        // RangeFromChild fails if `link` isn't a direct child
                        // of this provider, so fall back to the range at the
                        // cursor, which still covers the link's visible text.
                        var text = textPattern.RangeFromChild(link)?.GetText(Config.MaxTextLength);
                        if (string.IsNullOrWhiteSpace(text))
                            text = textPattern.RangeFromPoint(point)?.GetText(Config.MaxTextLength);
                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }
                    catch { /* try the next ancestor */ }
                }
                current = current.Parent;
            }
            return null;
        }

        private static readonly Regex UrlRegex =
            new(Config.UrlNameRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static bool LooksLikeUrl(string? name) =>
            !string.IsNullOrWhiteSpace(name) && UrlRegex.IsMatch(name);

        // Browsers nest the rendered link text in a Text element inside the
        // hyperlink. Search a few levels down for the first such element whose
        // Name holds actual text — that's exactly what's shown on screen.
        private static string? GetDescendantText(AutomationElement link)
        {
            var textCondition = link.ConditionFactory.ByControlType(ControlType.Text);
            var level = new List<AutomationElement> { link };
            for (var depth = 0; depth < 3 && level.Count > 0; depth++)
            {
                var next = new List<AutomationElement>();
                foreach (var current in level)
                {
                    try
                    {
                        foreach (var child in current.FindAllChildren(textCondition))
                        {
                            var text = child.Name;
                            if (!string.IsNullOrWhiteSpace(text))
                                return text;
                            next.Add(child);
                        }
                    }
                    catch { }
                }
                level = next;
            }
            return null;
        }

        public static void Dispose() => Automation.Dispose();
    }
}
