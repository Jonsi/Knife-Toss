using System;
using Game.UI.Tokens;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.UI
{
    [CustomEditor(typeof(UiSemanticTokens))]
    public sealed class UiSemanticTokensEditor : UnityEditor.Editor
    {
        private static readonly string[] SectionProperties =
        {
            "_Colors",
            "_Typography",
            "_Layout",
            "_Surfaces",
            "_Buttons",
            "_Motion",
            "_Layers",
            "_Accessibility"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var tokens = (UiSemanticTokens)target;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Hand-Carved Forest Arcade", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Semantic presentation policy only — no gameplay, economy, store, or navigation values.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(6f);

            DrawValidation(tokens);
            DrawColorGrid(tokens);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_SchemaVersion"));
            foreach (string propertyName in SectionProperties)
            {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                if (property != null)
                    EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawValidation(UiSemanticTokens tokens)
        {
            var issues = tokens.GetValidationIssues();
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"Complete and valid · {Enum.GetValues(typeof(UiColorRole)).Length} colors · " +
                    $"{Enum.GetValues(typeof(UiTypographyRole)).Length} typography roles · " +
                    $"{Enum.GetValues(typeof(UiButtonStyle)).Length - 1} button surfaces",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(string.Join("\n", issues), MessageType.Error);
        }

        private static void DrawColorGrid(UiSemanticTokens tokens)
        {
            EditorGUILayout.LabelField("Semantic Colors", EditorStyles.boldLabel);
            foreach (UiColorRole role in Enum.GetValues(typeof(UiColorRole)))
            {
                Rect row = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2f);
                Rect swatch = new Rect(row.x, row.y + 1f, 44f, row.height - 2f);
                Rect label = new Rect(row.x + 50f, row.y, row.width - 50f, row.height);
                Color color = tokens.Colors.Get(role);

                EditorGUI.DrawRect(swatch, color);
                Handles.DrawSolidRectangleWithOutline(swatch, Color.clear, new Color(0f, 0f, 0f, 0.45f));
                EditorGUI.LabelField(label, $"{role}  #{ColorUtility.ToHtmlStringRGBA(color)}");
            }

            EditorGUILayout.Space(6f);
        }
    }
}
