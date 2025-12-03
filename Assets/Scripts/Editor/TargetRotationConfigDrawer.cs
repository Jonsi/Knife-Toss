using KnifeToss.Core.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KnifeToss.Editor
{
    [CustomPropertyDrawer(typeof(TargetRotationConfig))]
    public sealed class TargetRotationConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            foreach (SerializedProperty prop in EnumerateVisibleProperties(property))
            {
                float propHeight = EditorGUI.GetPropertyHeight(prop, true);
                row.height = propHeight;

                bool enabled = IsEnabled(prop.name, property);
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    EditorGUI.PropertyField(row, prop, new GUIContent(ObjectNames.NicifyVariableName(prop.name)), true);
                }

                row.y += propHeight + spacing;
            }

            EditorGUI.EndProperty();
        }

        private static IEnumerable<SerializedProperty> EnumerateVisibleProperties(SerializedProperty property)
        {
            var speed = property.FindPropertyRelative(nameof(TargetRotationConfig.speed));
            var type = property.FindPropertyRelative(nameof(TargetRotationConfig.behaviourType));

            if (speed != null) yield return speed;
            if (type != null) yield return type;

            RotationBehaviourType behaviourType = type != null ? (RotationBehaviourType)type.enumValueIndex : RotationBehaviourType.Constant;
            switch (behaviourType)
            {
                case RotationBehaviourType.Pulsating:
                    yield return property.FindPropertyRelative(nameof(TargetRotationConfig.pulsating));
                    break;
                case RotationBehaviourType.Flipping:
                    yield return property.FindPropertyRelative(nameof(TargetRotationConfig.flipping));
                    break;
                case RotationBehaviourType.AccelerationCurve:
                    yield return property.FindPropertyRelative(nameof(TargetRotationConfig.acceleration));
                    break;
            }
        }

        private static bool IsEnabled(string propertyName, SerializedProperty parent)
        {
            SerializedProperty typeProp = parent.FindPropertyRelative(nameof(TargetRotationConfig.behaviourType));
            RotationBehaviourType type = typeProp != null ? (RotationBehaviourType)typeProp.enumValueIndex : RotationBehaviourType.Constant;

            if (propertyName == nameof(TargetRotationConfig.pulsating))
                return type == RotationBehaviourType.Pulsating;
            if (propertyName == nameof(TargetRotationConfig.flipping))
                return type == RotationBehaviourType.Flipping;
            if (propertyName == nameof(TargetRotationConfig.acceleration))
                return type == RotationBehaviourType.AccelerationCurve;

            return true;
        }
    }
}
