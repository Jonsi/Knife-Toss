using System.Collections.Generic;
using Core.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Custom inspector for LevelConfig that provides a cleaner authoring workflow.
    /// </summary>
    [CustomEditor(typeof(LevelConfig))]
    public sealed class LevelConfigEditor : UnityEditor.Editor
    {
        private ReorderableList levelList;
        private SerializedProperty levelsProp;

        private readonly Dictionary<int, ReorderableList> targetLists = new Dictionary<int, ReorderableList>();
        private readonly Dictionary<int, int> selectedTargetIndex = new Dictionary<int, int>();
        private readonly Dictionary<int, bool> targetFoldoutStates = new Dictionary<int, bool>();
        private readonly Dictionary<string, ReorderableList> hitableLists = new Dictionary<string, ReorderableList>();

        private int selectedLevel = -1;

        private void OnEnable()
        {
            levelsProp = serializedObject.FindProperty(nameof(LevelConfig.levels));

            levelList = new ReorderableList(serializedObject, levelsProp, true, true, true, true);
            levelList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Levels");
            levelList.onSelectCallback = list =>
            {
                selectedLevel = list.index;
                GUI.FocusControl(null);
            };
            levelList.onAddCallback = list =>
            {
                int newIndex = levelsProp.arraySize;
                levelsProp.InsertArrayElementAtIndex(newIndex);
                SerializedProperty element = levelsProp.GetArrayElementAtIndex(newIndex);
                element.FindPropertyRelative(nameof(LevelData.knives)).intValue = 0;
                selectedLevel = newIndex;
            };
            levelList.drawElementCallback = DrawLevelElement;
            levelList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight + 4f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            levelList.DoLayoutList();

            if (selectedLevel >= 0 && selectedLevel < levelsProp.arraySize)
            {
                SerializedProperty levelProp = levelsProp.GetArrayElementAtIndex(selectedLevel);
                DrawLevelDetails(levelProp, selectedLevel);
            }
            else if (levelsProp.arraySize > 0)
            {
                EditorGUILayout.HelpBox("Select a level in the list to edit its details.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No levels configured. Use the + button above to add one.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLevelElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= levelsProp.arraySize)
            {
                return;
            }

            SerializedProperty element = levelsProp.GetArrayElementAtIndex(index);
            SerializedProperty targetGroupProp = element.FindPropertyRelative(nameof(LevelData.targetGroup));
            SerializedProperty targetsProp = targetGroupProp?.FindPropertyRelative(nameof(TargetGroupConfig.targets));
            int targetCount = targetsProp != null ? targetsProp.arraySize : 0;

            int calculatedKnives = 0;
            if (targetsProp != null)
            {
                for (int i = 0; i < targetsProp.arraySize; i++)
                {
                    SerializedProperty target = targetsProp.GetArrayElementAtIndex(i);
                    SerializedProperty knivesToBreak = target.FindPropertyRelative(nameof(TargetDefinition.knivesToBreak));
                    if (knivesToBreak != null)
                    {
                        calculatedKnives += Mathf.Max(0, knivesToBreak.intValue);
                    }
                }
            }

            string label = $"Level {index + 1}  •  Knives: {calculatedKnives} (calculated)  •  Targets: {targetCount}";
            EditorGUI.LabelField(rect, label);
        }

        private void DrawLevelDetails(SerializedProperty levelProp, int levelIndex)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField($"Level {levelIndex + 1} Details", EditorStyles.boldLabel);

                SerializedProperty targetGroup = levelProp.FindPropertyRelative(nameof(LevelData.targetGroup));
                SerializedProperty targets = targetGroup?.FindPropertyRelative(nameof(TargetGroupConfig.targets));
                int calculatedKnives = 0;
                if (targets != null)
                {
                    for (int i = 0; i < targets.arraySize; i++)
                    {
                        SerializedProperty target = targets.GetArrayElementAtIndex(i);
                        SerializedProperty knivesToBreak = target.FindPropertyRelative(nameof(TargetDefinition.knivesToBreak));
                        if (knivesToBreak != null)
                        {
                            calculatedKnives += Mathf.Max(0, knivesToBreak.intValue);
                        }
                    }
                }
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Total Knives (Calculated)", calculatedKnives);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Total knives are automatically calculated from the sum of all targets' 'Knives To Break' values.", MessageType.Info);

                if (targetGroup != null)
                {
                    DrawTargetsSection(levelIndex, targets);
                }
            }
        }

        private void DrawTargetsSection(int levelIndex, SerializedProperty targetsProp)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);

                ReorderableList targetList = GetOrCreateTargetList(levelIndex, targetsProp);
                targetList.DoLayoutList();

                DrawSelectedTarget(levelIndex, targetsProp);
            }
        }

        private ReorderableList GetOrCreateTargetList(int levelIndex, SerializedProperty targetsProp)
        {
            if (targetsProp == null)
            {
                return null;
            }

            if (targetLists.TryGetValue(levelIndex, out ReorderableList existing))
            {
                existing.serializedProperty = targetsProp;
                return existing;
            }

            ReorderableList list = new ReorderableList(targetsProp.serializedObject, targetsProp, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Target Definitions");
            list.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty target = targetsProp.GetArrayElementAtIndex(index);
                SerializedProperty prefab = target.FindPropertyRelative(nameof(TargetDefinition.prefab));
                SerializedProperty orbit = target.FindPropertyRelative(nameof(TargetDefinition.orbitRadius));
                SerializedProperty quota = target.FindPropertyRelative(nameof(TargetDefinition.knivesToBreak));
                string prefabName = prefab.objectReferenceValue != null ? prefab.objectReferenceValue.name : "Default Prefab";
                string label = $"Target {index + 1}  •  {prefabName}  •  Orbit {orbit.floatValue:0.##}  •  Knives To Break {quota.intValue}";
                EditorGUI.LabelField(rect, label);
            };
            list.onSelectCallback = l =>
            {
                selectedTargetIndex[levelIndex] = l.index;
                targetFoldoutStates[levelIndex] = true;
                GUI.FocusControl(null);
            };
            list.onAddCallback = l =>
            {
                int newIndex = targetsProp.arraySize;
                targetsProp.InsertArrayElementAtIndex(newIndex);
                selectedTargetIndex[levelIndex] = newIndex;
                targetFoldoutStates[levelIndex] = true;
            };

            targetLists[levelIndex] = list;
            return list;
        }

        private void DrawSelectedTarget(int levelIndex, SerializedProperty targetsProp)
        {
            if (!selectedTargetIndex.TryGetValue(levelIndex, out int targetIndex)
                || targetIndex < 0
                || targetIndex >= targetsProp.arraySize)
            {
                if (targetsProp.arraySize > 0)
                {
                    EditorGUILayout.HelpBox("Select a target from the list above to edit its properties.", MessageType.None);
                }
                return;
            }

            SerializedProperty targetProp = targetsProp.GetArrayElementAtIndex(targetIndex);
            bool foldoutState = targetFoldoutStates.TryGetValue(levelIndex, out bool state) ? state : true;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Rect rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);
                foldoutState = EditorGUI.Foldout(rect, foldoutState, $"Target {targetIndex + 1}", true);
                targetFoldoutStates[levelIndex] = foldoutState;

                if (!foldoutState)
                {
                    return;
                }

                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.prefab)));
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.orbitRadius)));
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.startingAngle)));
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.orbitDegreesPerSecond)));
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.baseScale)));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Pre-Stuck Knives", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.preStuckKnives)));
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.preStuckKnifeSpacing)));

                EditorGUILayout.Space();
                DrawRotationSection(targetProp.FindPropertyRelative(nameof(TargetDefinition.rotation)));

                EditorGUILayout.Space();
                DrawHitableSection(levelIndex, targetIndex, targetProp.FindPropertyRelative(nameof(TargetDefinition.hitableObjects)));

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.knivesToBreak)));
            }
        }

        private static void DrawRotationSection(SerializedProperty rotationProp)
        {
            if (rotationProp == null)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(rotationProp.FindPropertyRelative(nameof(TargetRotationConfig.speed)));
                EditorGUILayout.PropertyField(rotationProp.FindPropertyRelative(nameof(TargetRotationConfig.behaviourType)));

                RotationBehaviourType behaviour = (RotationBehaviourType)rotationProp.FindPropertyRelative(nameof(TargetRotationConfig.behaviourType)).enumValueIndex;
                switch (behaviour)
                {
                    case RotationBehaviourType.Pulsating:
                        EditorGUILayout.PropertyField(rotationProp.FindPropertyRelative(nameof(TargetRotationConfig.pulsating)), true);
                        break;
                    case RotationBehaviourType.Flipping:
                        EditorGUILayout.PropertyField(rotationProp.FindPropertyRelative(nameof(TargetRotationConfig.flipping)), true);
                        break;
                    case RotationBehaviourType.AccelerationCurve:
                        EditorGUILayout.PropertyField(rotationProp.FindPropertyRelative(nameof(TargetRotationConfig.acceleration)), true);
                        break;
                }
            }
        }

        private void DrawHitableSection(int levelIndex, int targetIndex, SerializedProperty hitablesProp)
        {
            if (hitablesProp == null)
            {
                return;
            }

            string key = $"{levelIndex}_{targetIndex}";
            if (!hitableLists.TryGetValue(key, out ReorderableList list))
            {
                list = new ReorderableList(hitablesProp.serializedObject, hitablesProp, true, true, true, true);
                list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Hitable Objects");
                list.elementHeightCallback = idx =>
                {
                    if (idx < 0 || idx >= hitablesProp.arraySize)
                    {
                        return EditorGUIUtility.singleLineHeight;
                    }

                    SerializedProperty element = hitablesProp.GetArrayElementAtIndex(idx);
                    return EditorGUI.GetPropertyHeight(element, true) + 4f;
                };

                list.drawElementCallback = (rect, index, active, focused) =>
                {
                    if (index < 0 || index >= hitablesProp.arraySize)
                    {
                        return;
                    }

                    SerializedProperty element = hitablesProp.GetArrayElementAtIndex(index);
                    rect.height = EditorGUI.GetPropertyHeight(element, true);
                    EditorGUI.PropertyField(rect, element, GUIContent.none, true);
                };

                hitableLists[key] = list;
            }

            list.serializedProperty = hitablesProp;
            list.DoLayoutList();
        }
    }
}
