using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextAsset))]
[CanEditMultipleObjects]
public class MarkdownInspector : Editor
{
    private Vector2 scrollPosition;
    private bool showRawText = false;
    
    public override void OnInspectorGUI()
    {
        TextAsset textAsset = (TextAsset)target;
        
        // Check if this is a markdown file
        string assetPath = AssetDatabase.GetAssetPath(textAsset);
        if (!assetPath.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
        {
            // Not a markdown file, use default inspector
            DrawDefaultInspector();
            return;
        }
        
        // Display markdown content
        EditorGUILayout.LabelField("Markdown Content", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Toggle to show raw text
        showRawText = EditorGUILayout.Toggle("Show Raw Text", showRawText);
        EditorGUILayout.Space();
        
        // Create a scrollable text area
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        
        if (showRawText)
        {
            // Show raw text with word wrap
            EditorStyles.textArea.wordWrap = true;
            EditorGUILayout.TextArea(textAsset.text, EditorStyles.textArea, GUILayout.ExpandHeight(true));
        }
        else
        {
            // Show formatted markdown (basic formatting)
            string formattedText = FormatMarkdown(textAsset.text);
            EditorStyles.textArea.wordWrap = true;
            EditorGUILayout.TextArea(formattedText, EditorStyles.textArea, GUILayout.ExpandHeight(true));
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        // Display file info
        EditorGUILayout.LabelField("File Info", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Path", assetPath);
        EditorGUILayout.IntField("Character Count", textAsset.text.Length);
        EditorGUILayout.IntField("Line Count", textAsset.text.Split('\n').Length);
        EditorGUI.EndDisabledGroup();
    }
    
    private string FormatMarkdown(string markdown)
    {
        // Basic markdown formatting for display
        // This is a simplified formatter - Unity's text area doesn't support rich text formatting
        // but we can at least make it more readable
        
        string formatted = markdown;
        
        // Remove markdown syntax for headers (since we can't style them in a text area)
        // Keep the text but remove the # symbols for better readability
        formatted = System.Text.RegularExpressions.Regex.Replace(
            formatted, 
            @"^#{1,6}\s+(.+)$", 
            "$1", 
            System.Text.RegularExpressions.RegexOptions.Multiline
        );
        
        // Remove bold/italic markers (keep text)
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"\*\*([^*]+)\*\*", "$1");
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"\*([^*]+)\*", "$1");
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"__([^_]+)__", "$1");
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"_([^_]+)_", "$1");
        
        // Remove code block markers
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"```[\s\S]*?```", "[Code Block]");
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"`([^`]+)`", "$1");
        
        // Remove link syntax but keep text
        formatted = System.Text.RegularExpressions.Regex.Replace(formatted, @"\[([^\]]+)\]\([^\)]+\)", "$1");
        
        return formatted;
    }
}

