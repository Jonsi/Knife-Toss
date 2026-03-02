#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Store.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    /// <summary>
    /// Inspector drawer for SerializeReference fields with a concrete type dropdown.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializeReferenceDropdownAttribute))]
    public class SerializeReferenceDropdownDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, Type[]> DerivedTypeCache = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<int, Dictionary<long, string>> ManagedReferenceOwners = new Dictionary<int, Dictionary<long, string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var baseType = fieldInfo.FieldType;
            var derivedTypes = GetDerivedTypes(baseType);
            var typeNames = new[] { "(None)" }.Concat(derivedTypes.Select(type => type.Name)).ToArray();

            var currentType = property.managedReferenceValue?.GetType();
            var currentIndex = currentType == null ? 0 : Array.IndexOf(derivedTypes, currentType) + 1;

            var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, typeNames);

            if (newIndex != currentIndex)
            {
                property.managedReferenceValue = newIndex == 0
                    ? null
                    : Activator.CreateInstance(derivedTypes[newIndex - 1]);
                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.managedReferenceValue != null)
            {
                EnsureUniqueManagedReferenceInstance(property);
                DrawChildren(position, property);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (property.managedReferenceValue == null)
            {
                return height;
            }

            var iterator = property.Copy();
            var end = iterator.GetEndProperty();

            if (!iterator.NextVisible(true))
            {
                return height;
            }

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(iterator, true);

                if (!iterator.NextVisible(false))
                {
                    break;
                }
            }

            return height;
        }

        private static Type[] GetDerivedTypes(Type baseType)
        {
            if (baseType == null)
            {
                return Array.Empty<Type>();
            }

            if (DerivedTypeCache.TryGetValue(baseType, out var cached))
            {
                return cached;
            }

            var allTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(type => type != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (type == null)
                    {
                        continue;
                    }

                    if (!baseType.IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    allTypes.Add(type);
                }
            }

            var result = allTypes.OrderBy(type => type.Name).ToArray();
            DerivedTypeCache[baseType] = result;
            return result;
        }

        private static void EnsureUniqueManagedReferenceInstance(SerializedProperty property)
        {
            if (property == null || property.managedReferenceValue == null)
            {
                return;
            }

            var targetObject = property.serializedObject.targetObject;
            if (targetObject == null)
            {
                return;
            }

            var targetId = targetObject.GetInstanceID();
            if (!ManagedReferenceOwners.TryGetValue(targetId, out var owners))
            {
                owners = new Dictionary<long, string>();
                ManagedReferenceOwners[targetId] = owners;
            }

            var referenceId = property.managedReferenceId;
            if (referenceId == 0)
            {
                return;
            }

            if (owners.TryGetValue(referenceId, out var existingPath))
            {
                if (existingPath == property.propertyPath)
                {
                    return;
                }

                property.managedReferenceValue = CloneManagedReference(property.managedReferenceValue);
                property.serializedObject.ApplyModifiedProperties();
                referenceId = property.managedReferenceId;
            }

            owners[referenceId] = property.propertyPath;
        }

        private static object CloneManagedReference(object source)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();
            var json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson(json, type);
        }

        private static void DrawChildren(Rect position, SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = iterator.GetEndProperty();

            if (!iterator.NextVisible(true))
            {
                return;
            }

            var y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            while (!SerializedProperty.EqualContents(iterator, end))
            {
                var height = EditorGUI.GetPropertyHeight(iterator, true);
                var rect = new Rect(position.x, y, position.width, height);
                EditorGUI.PropertyField(rect, iterator, true);
                y += height + EditorGUIUtility.standardVerticalSpacing;

                if (!iterator.NextVisible(false))
                {
                    break;
                }
            }
        }
    }
}
#endif
