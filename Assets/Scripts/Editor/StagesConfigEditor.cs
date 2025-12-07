using System.Collections.Generic;
using Core.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Custom inspector for StagesConfig that provides a cleaner authoring workflow.
    /// </summary>
    [CustomEditor(typeof(StagesConfig))]
    public sealed class StagesConfigEditor : UnityEditor.Editor
    {
        private ReorderableList stageList;
        private SerializedProperty stagesProp;

        private readonly Dictionary<int, ReorderableList> levelLists = new Dictionary<int, ReorderableList>();
        private readonly Dictionary<int, int> selectedLevelIndex = new Dictionary<int, int>();
        private readonly Dictionary<int, bool> levelFoldoutStates = new Dictionary<int, bool>();
        private readonly Dictionary<string, ReorderableList> targetLists = new Dictionary<string, ReorderableList>();
        private readonly Dictionary<string, int> selectedTargetIndex = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> targetFoldoutStates = new Dictionary<string, bool>();
        private readonly Dictionary<string, ReorderableList> hitableLists = new Dictionary<string, ReorderableList>();

        private int selectedStage = -1;

        private void OnEnable()
        {
            stagesProp = serializedObject.FindProperty(nameof(StagesConfig.stages));

            stageList = new ReorderableList(serializedObject, stagesProp, true, true, true, true);
            stageList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Stages");
            stageList.onSelectCallback = list =>
            {
                selectedStage = list.index;
                GUI.FocusControl(null);
            };
            stageList.onAddCallback = list =>
            {
                int newIndex = stagesProp.arraySize;
                stagesProp.InsertArrayElementAtIndex(newIndex);
                SerializedProperty element = stagesProp.GetArrayElementAtIndex(newIndex);
                element.FindPropertyRelative(nameof(StageData.stageName)).stringValue = $"Stage {newIndex + 1}";
                element.FindPropertyRelative(nameof(StageData.levels)).arraySize = 0;
                selectedStage = newIndex;
            };
            stageList.drawElementCallback = DrawStageElement;
            stageList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight + 4f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            stageList.DoLayoutList();

            if (selectedStage >= 0 && selectedStage < stagesProp.arraySize)
            {
                SerializedProperty stageProp = stagesProp.GetArrayElementAtIndex(selectedStage);
                DrawStageDetails(stageProp, selectedStage);
            }
            else if (stagesProp.arraySize > 0)
            {
                EditorGUILayout.HelpBox("Select a stage in the list to edit its details.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No stages configured. Use the + button above to add one.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStageElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= stagesProp.arraySize)
            {
                return;
            }

            SerializedProperty element = stagesProp.GetArrayElementAtIndex(index);
            SerializedProperty stageNameProp = element.FindPropertyRelative(nameof(StageData.stageName));
            SerializedProperty levelsProp = element.FindPropertyRelative(nameof(StageData.levels));
            int levelCount = levelsProp != null ? levelsProp.arraySize : 0;

            string stageName = stageNameProp != null ? stageNameProp.stringValue : $"Stage {index + 1}";
            string label = $"{stageName}  •  {levelCount} levels";
            EditorGUI.LabelField(rect, label);
        }

        private void DrawStageDetails(SerializedProperty stageProp, int stageIndex)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField($"Stage {stageIndex + 1} Details", EditorStyles.boldLabel);

                SerializedProperty stageNameProp = stageProp.FindPropertyRelative(nameof(StageData.stageName));
                EditorGUILayout.PropertyField(stageNameProp);

                SerializedProperty levelsProp = stageProp.FindPropertyRelative(nameof(StageData.levels));
                if (levelsProp != null)
                {
                    DrawLevelsSection(stageIndex, levelsProp);
                }
            }
        }

        private void DrawLevelsSection(int stageIndex, SerializedProperty levelsProp)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

                ReorderableList levelList = GetOrCreateLevelList(stageIndex, levelsProp);
                levelList.DoLayoutList();

                DrawSelectedLevel(stageIndex, levelsProp);
            }
        }

        private ReorderableList GetOrCreateLevelList(int stageIndex, SerializedProperty levelsProp)
        {
            if (levelsProp == null)
            {
                return null;
            }

            if (levelLists.TryGetValue(stageIndex, out ReorderableList existing))
            {
                existing.serializedProperty = levelsProp;
                return existing;
            }

            ReorderableList list = new ReorderableList(levelsProp.serializedObject, levelsProp, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Level Definitions");
            list.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty level = levelsProp.GetArrayElementAtIndex(index);
                SerializedProperty targetGroupProp = level.FindPropertyRelative(nameof(LevelData.targetGroup));
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
            };
            list.onSelectCallback = l =>
            {
                selectedLevelIndex[stageIndex] = l.index;
                levelFoldoutStates[stageIndex] = true;
                GUI.FocusControl(null);
            };
            list.onAddCallback = l =>
            {
                int newIndex = levelsProp.arraySize;
                levelsProp.InsertArrayElementAtIndex(newIndex);
                SerializedProperty element = levelsProp.GetArrayElementAtIndex(newIndex);
                element.FindPropertyRelative(nameof(LevelData.knives)).intValue = 0;
                selectedLevelIndex[stageIndex] = newIndex;
                levelFoldoutStates[stageIndex] = true;
            };

            levelLists[stageIndex] = list;
            return list;
        }

        private void DrawSelectedLevel(int stageIndex, SerializedProperty levelsProp)
        {
            if (!selectedLevelIndex.TryGetValue(stageIndex, out int levelIndex)
                || levelIndex < 0
                || levelIndex >= levelsProp.arraySize)
            {
                if (levelsProp.arraySize > 0)
                {
                    EditorGUILayout.HelpBox("Select a level in the list above to edit its details.", MessageType.None);
                }
                return;
            }

            SerializedProperty levelProp = levelsProp.GetArrayElementAtIndex(levelIndex);
            bool foldoutState = levelFoldoutStates.TryGetValue(stageIndex, out bool state) ? state : true;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Rect rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);
                foldoutState = EditorGUI.Foldout(rect, foldoutState, $"Level {levelIndex + 1}", true);
                levelFoldoutStates[stageIndex] = foldoutState;

                if (!foldoutState)
                {
                    return;
                }

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
                    DrawTargetsSection(stageIndex, levelIndex, targets);
                }
            }
        }

        private void DrawTargetsSection(int stageIndex, int levelIndex, SerializedProperty targetsProp)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);

                string key = $"{stageIndex}_{levelIndex}";
                ReorderableList targetList = GetOrCreateTargetList(key, targetsProp);
                targetList.DoLayoutList();

                DrawSelectedTarget(stageIndex, levelIndex, targetsProp);
            }
        }

        private ReorderableList GetOrCreateTargetList(string key, SerializedProperty targetsProp)
        {
            if (targetsProp == null)
            {
                return null;
            }

            if (targetLists.TryGetValue(key, out ReorderableList existing))
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
                selectedTargetIndex[key] = l.index;
                targetFoldoutStates[key] = true;
                GUI.FocusControl(null);
            };
            list.onAddCallback = l =>
            {
                int newIndex = targetsProp.arraySize;
                targetsProp.InsertArrayElementAtIndex(newIndex);
                selectedTargetIndex[key] = newIndex;
                targetFoldoutStates[key] = true;
            };

            targetLists[key] = list;
            return list;
        }

        private void DrawSelectedTarget(int stageIndex, int levelIndex, SerializedProperty targetsProp)
        {
            string key = $"{stageIndex}_{levelIndex}";
            if (!selectedTargetIndex.TryGetValue(key, out int targetIndex)
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
            bool foldoutState = targetFoldoutStates.TryGetValue(key, out bool state) ? state : true;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                Rect rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);
                foldoutState = EditorGUI.Foldout(rect, foldoutState, $"Target {targetIndex + 1}", true);
                targetFoldoutStates[key] = foldoutState;

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
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.preStuckKnifeSpacingOffset)));

                EditorGUILayout.Space();
                DrawRotationSection(targetProp.FindPropertyRelative(nameof(TargetDefinition.rotation)));

                EditorGUILayout.Space();
                DrawHitableSection(stageIndex, levelIndex, targetIndex, targetProp.FindPropertyRelative(nameof(TargetDefinition.hitableObjects)));
                EditorGUILayout.PropertyField(targetProp.FindPropertyRelative(nameof(TargetDefinition.hitableObjectSpacingOffset)));

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

        private void DrawHitableSection(int stageIndex, int levelIndex, int targetIndex, SerializedProperty hitablesProp)
        {
            if (hitablesProp == null)
            {
                return;
            }

            string key = $"{stageIndex}_{levelIndex}_{targetIndex}";
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

