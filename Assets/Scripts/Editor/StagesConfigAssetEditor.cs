using Game.Level.Catalogs;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Custom inspector for StagesConfigAsset adds a reorderable list with dynamic heights.
    /// </summary>
    [CustomEditor(typeof(StagesConfigAsset))]
    public class StagesConfigAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _defaultTargetPrefab;
        private SerializedProperty _stages;
        private ReorderableList _stagesList;

        private void OnEnable()
        {
            _defaultTargetPrefab = serializedObject.FindProperty("<DefaultTargetPrefab>k__BackingField");
            _stages = serializedObject.FindProperty("<Stages>k__BackingField");

            if (_stages == null)
            {
                return;
            }

            _stagesList = new ReorderableList(serializedObject, _stages, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Stages"),
                elementHeightCallback = GetStageElementHeight,
                drawElementCallback = DrawStageElement
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_defaultTargetPrefab != null)
            {
                EditorGUILayout.PropertyField(_defaultTargetPrefab);
                EditorGUILayout.Space();
            }

            if (_stagesList != null)
            {
                _stagesList.DoLayoutList();
            }
            else if (_stages != null)
            {
                EditorGUILayout.PropertyField(_stages);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private float GetStageElementHeight(int index)
        {
            SerializedProperty element = _stages.GetArrayElementAtIndex(index);
            // Add padding so the selection background uses the full height.
            return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing * 2f;
        }

        private void DrawStageElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _stages.GetArrayElementAtIndex(index);

            rect.y += EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUI.GetPropertyHeight(element, true);

            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        }
    }
}
