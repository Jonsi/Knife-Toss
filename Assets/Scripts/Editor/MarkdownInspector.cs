using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Represents the MarkdownInspector class.
/// </summary>
[CustomEditor(typeof(TextAsset))]
[CanEditMultipleObjects]
public class MarkdownInspector : Editor
{
    private Vector2 scrollPosition;
    private bool stylesInitialized;
    private bool cachedDarkSkin;

    private GUIStyle header1Style;
    private GUIStyle header2Style;
    private GUIStyle header3Style;
    private GUIStyle normalStyle;
    private GUIStyle listStyle;
    private GUIStyle quoteStyle;
    private GUIStyle codeStyle;

    private Color horizontalRuleColor;
    private Color quoteStripeColor;
    private Color codeBackgroundColor;
    private string inlineCodeColor;

    /// <summary>
    /// Performs the OnInspectorGUI operation.
    /// </summary>
    public override void OnInspectorGUI()
    {
        var textAsset = (TextAsset)target;

        string assetPath = AssetDatabase.GetAssetPath(textAsset);
        if (!assetPath.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
        {
            DrawDefaultInspector();
            return;
        }

        EnsureStyles();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        RenderMarkdown(textAsset.text ?? string.Empty);
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Performs the RenderMarkdown operation.
    /// </summary>
    private void RenderMarkdown(string markdown)
    {
        string[] lines = markdown.Replace("\r", string.Empty).Split('\n');
        bool inCodeBlock = false;
        List<string> codeLines = new List<string>();
        List<string> paragraphBuffer = new List<string>();
        bool inOrderedList = false;
        int orderedListNumber = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.Trim();

            if (trimmed.StartsWith("```"))
            {
                FlushParagraph(paragraphBuffer);

                if (inCodeBlock)
                {
                    DrawCodeBlock(string.Join("\n", codeLines));
                    codeLines.Clear();
                }

                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                codeLines.Add(line);
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                FlushParagraph(paragraphBuffer);
                if (inOrderedList)
                {
                    inOrderedList = false;
                    orderedListNumber = 0;
                }

                EditorGUILayout.Space(4);
                continue;
            }

            if (Regex.IsMatch(trimmed, @"^(-{3,}|_{3,}|\*{3,})$"))
            {
                FlushParagraph(paragraphBuffer);
                DrawHorizontalRule();
                continue;
            }

            Match headerMatch = Regex.Match(trimmed, @"^(#{1,6})\s+(.+)$");
            if (headerMatch.Success)
            {
                FlushParagraph(paragraphBuffer);
                if (inOrderedList)
                {
                    inOrderedList = false;
                    orderedListNumber = 0;
                }

                int headerLevel = Mathf.Clamp(headerMatch.Groups[1].Value.Length, 1, 3);
                string headerText = ProcessInlineFormatting(headerMatch.Groups[2].Value.Trim());
                DrawHeader(headerLevel, headerText);
                continue;
            }

            if (trimmed.StartsWith(">"))
            {
                FlushParagraph(paragraphBuffer);
                string quoteText = ProcessInlineFormatting(trimmed.TrimStart('>').Trim());
                DrawQuote(quoteText);
                continue;
            }

            Match orderedMatch = Regex.Match(trimmed, @"^(\d+)\.\s+(.+)$");
            if (orderedMatch.Success)
            {
                FlushParagraph(paragraphBuffer);
                if (!inOrderedList)
                {
                    inOrderedList = true;
                    orderedListNumber = 0;
                }

                orderedListNumber++;
                string listText = ProcessInlineFormatting(orderedMatch.Groups[2].Value.Trim());
                DrawSelectableLabel($"{orderedListNumber}. {listText}", listStyle);
                continue;
            }

            if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
            {
                FlushParagraph(paragraphBuffer);
                string listText = ProcessInlineFormatting(trimmed.Substring(2).Trim());
                DrawSelectableLabel($"• {listText}", listStyle);
                continue;
            }

            paragraphBuffer.Add(trimmed);
        }

        FlushParagraph(paragraphBuffer);

        if (inCodeBlock && codeLines.Count > 0)
        {
            DrawCodeBlock(string.Join("\n", codeLines));
        }
    }

    /// <summary>
    /// Performs the EnsureStyles operation.
    /// </summary>
    private void EnsureStyles()
    {
        bool isDark = EditorGUIUtility.isProSkin;
        if (stylesInitialized && cachedDarkSkin == isDark)
        {
            return;
        }

        cachedDarkSkin = isDark;

        Color bodyText = isDark ? new Color(0.88f, 0.9f, 0.92f) : new Color(0.12f, 0.12f, 0.12f);
        Color secondaryText = isDark ? new Color(0.76f, 0.79f, 0.82f) : new Color(0.2f, 0.2f, 0.2f);
        horizontalRuleColor = isDark ? new Color(0.32f, 0.32f, 0.32f) : new Color(0.75f, 0.75f, 0.75f);
        quoteStripeColor = isDark ? new Color(0.29f, 0.55f, 0.88f) : new Color(0.12f, 0.35f, 0.72f);
        codeBackgroundColor = isDark ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.92f, 0.92f, 0.92f);
        inlineCodeColor = isDark ? "#FFD479" : "#5A2D00";

        normalStyle = CreateBaseStyle(EditorStyles.label, bodyText);
        normalStyle.fontSize = 13;
        normalStyle.margin = new RectOffset(4, 4, 2, 2);

        listStyle = CreateBaseStyle(EditorStyles.label, bodyText);
        listStyle.fontSize = 13;
        listStyle.margin = new RectOffset(24, 4, 2, 2);

        quoteStyle = CreateBaseStyle(EditorStyles.label, secondaryText);
        quoteStyle.fontSize = 13;
        quoteStyle.fontStyle = FontStyle.Italic;
        quoteStyle.margin = new RectOffset(12, 4, 3, 3);

        codeStyle = CreateBaseStyle(EditorStyles.textArea, bodyText);
        codeStyle.fontSize = 12;
        codeStyle.padding = new RectOffset(8, 8, 5, 5);
        codeStyle.margin = new RectOffset(10, 4, 4, 8);
        codeStyle.richText = false;

        header1Style = CreateHeaderStyle(bodyText, 24, FontStyle.Bold, new RectOffset(4, 4, 12, 6));
        header2Style = CreateHeaderStyle(bodyText, 18, FontStyle.Bold, new RectOffset(4, 4, 10, 4));
        header3Style = CreateHeaderStyle(bodyText, 15, FontStyle.Bold, new RectOffset(4, 4, 8, 4));

        stylesInitialized = true;
    }

    /// <summary>
    /// Performs the CreateBaseStyle operation.
    /// </summary>
    private GUIStyle CreateBaseStyle(GUIStyle template, Color textColor)
    {
        GUIStyle style = new GUIStyle(template)
        {
            richText = true,
            wordWrap = true,
            stretchWidth = true
        };

        style.normal.textColor = textColor;
        style.focused.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        style.onNormal.textColor = textColor;
        style.onFocused.textColor = textColor;
        style.onHover.textColor = textColor;
        style.onActive.textColor = textColor;

        return style;
    }

    /// <summary>
    /// Performs the CreateHeaderStyle operation.
    /// </summary>
    private GUIStyle CreateHeaderStyle(Color textColor, int fontSize, FontStyle fontStyle, RectOffset margin)
    {
        GUIStyle style = CreateBaseStyle(EditorStyles.boldLabel, textColor);
        style.fontSize = fontSize;
        style.fontStyle = fontStyle;
        style.margin = margin;
        return style;
    }

    /// <summary>
    /// Performs the DrawHeader operation.
    /// </summary>
    private void DrawHeader(int level, string text)
    {
        GUIStyle style = header3Style;
        if (level == 1)
        {
            style = header1Style;
        }
        else if (level == 2)
        {
            style = header2Style;
        }

        DrawSelectableLabel(text, style);

        if (level <= 2)
        {
            DrawHorizontalRule(level == 1 ? 6f : 4f);
        }
        else
        {
            EditorGUILayout.Space(4f);
        }
    }

    /// <summary>
    /// Performs the DrawQuote operation.
    /// </summary>
    private void DrawQuote(string text)
    {
        GUIContent content = new GUIContent(text);
        float width = GetContentWidth();
        float height = quoteStyle.CalcHeight(content, width);
        Rect rect = GUILayoutUtility.GetRect(content, quoteStyle, GUILayout.Height(height));

        Rect stripe = new Rect(rect.x, rect.y, 3f, rect.height);
        EditorGUI.DrawRect(stripe, quoteStripeColor);

        Rect textRect = new Rect(rect.x + 8f, rect.y, rect.width - 8f, rect.height);
        EditorGUI.SelectableLabel(textRect, text, quoteStyle);
    }

    /// <summary>
    /// Performs the DrawHorizontalRule operation.
    /// </summary>
    private void DrawHorizontalRule(float verticalPadding = 6f)
    {
        EditorGUILayout.Space(verticalPadding);
        Rect rect = EditorGUILayout.GetControlRect(false, 1f);
        rect.xMin += 4f;
        rect.xMax -= 4f;
        EditorGUI.DrawRect(rect, horizontalRuleColor);
        EditorGUILayout.Space(verticalPadding);
    }

    /// <summary>
    /// Performs the DrawCodeBlock operation.
    /// </summary>
    private void DrawCodeBlock(string text)
    {
        GUIContent content = new GUIContent(text);
        float width = GetContentWidth();
        float height = codeStyle.CalcHeight(content, width);
        Rect rect = GUILayoutUtility.GetRect(content, codeStyle, GUILayout.Height(height));
        EditorGUI.DrawRect(rect, codeBackgroundColor);
        Rect textRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        EditorGUI.SelectableLabel(textRect, text, codeStyle);
    }

    /// <summary>
    /// Performs the DrawSelectableLabel operation.
    /// </summary>
    private void DrawSelectableLabel(string text, GUIStyle style)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        GUIContent content = new GUIContent(text);
        float width = GetContentWidth();
        float height = style.CalcHeight(content, width);
        Rect rect = GUILayoutUtility.GetRect(content, style, GUILayout.Height(height));
        EditorGUI.SelectableLabel(rect, text, style);
    }

    /// <summary>
    /// Performs the FlushParagraph operation.
    /// </summary>
    private void FlushParagraph(List<string> paragraphBuffer)
    {
        if (paragraphBuffer.Count == 0)
        {
            return;
        }

        string text = string.Join(" ", paragraphBuffer);
        text = ProcessInlineFormatting(text.Trim());
        DrawSelectableLabel(text, normalStyle);
        paragraphBuffer.Clear();
    }

    /// <summary>
    /// Performs the GetContentWidth operation.
    /// </summary>
    private float GetContentWidth()
    {
        return Mathf.Max(10f, EditorGUIUtility.currentViewWidth - 32f);
    }

    /// <summary>
    /// Performs the ProcessInlineFormatting operation.
    /// </summary>
    private string ProcessInlineFormatting(string text)
    {
        text = Regex.Replace(text, @"\*\*([^*]+)\*\*", "<b>$1</b>");
        text = Regex.Replace(text, @"__([^_]+)__", "<b>$1</b>");

        text = Regex.Replace(text, @"(?<!\*)\*([^*]+)\*(?!\*)", "<i>$1</i>");
        text = Regex.Replace(text, @"(?<!_)_([^_]+)_(?!_)", "<i>$1</i>");

        text = Regex.Replace(text, @"`([^`]+)`", $"<color={inlineCodeColor}>$1</color>");

        text = Regex.Replace(text, @"\[([^\]]+)\]\([^\)]+\)", "$1");

        return text;
    }
}
