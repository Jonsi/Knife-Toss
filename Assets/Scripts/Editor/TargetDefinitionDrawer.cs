using KnifeToss.Core.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KnifeToss.Editor
{
    [CustomPropertyDrawer(typeof(TargetDefinition))]
    public sealed class TargetDefinitionDrawer : PropertyDrawer
    {
        /// <summary>
        /// Performs the GetPropertyHeight operation.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0f;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            foreach (SerializedProperty prop in EnumerateProperties(property))
            {
                height += EditorGUI.GetPropertyHeight(prop, true) + spacing;
            }

            return height;
        }

        /// <summary>
        /// Performs the OnGUI operation.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            foreach (SerializedProperty prop in EnumerateProperties(property))
            {
                float h = EditorGUI.GetPropertyHeight(prop, true);
                row.height = h;
                EditorGUI.PropertyField(row, prop, new GUIContent(ObjectNames.NicifyVariableName(prop.name)), true);
                row.y += h + spacing;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Performs the EnumerateProperties operation.
        /// </summary>
        private static IEnumerable<SerializedProperty> EnumerateProperties(SerializedProperty property)
        {
            yield return property.FindPropertyRelative(nameof(TargetDefinition.prefab));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.orbitRadius));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.startingAngle));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.orbitDegreesPerSecond));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.baseScale));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.preStuckKnives));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.preStuckKnifeSpacing));
            yield return property.FindPropertyRelative(nameof(TargetDefinition.rotation));
        }
    }
}
