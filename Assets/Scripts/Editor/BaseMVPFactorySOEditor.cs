using Common.MVP;
using UnityEditor;

namespace Game.Editor
{
    [CustomEditor(typeof(BaseMVPFactorySO<,,>), true)]
    public class BaseMVPFactorySOEditor : UnityEditor.Editor
    {
        private SerializedProperty _Prefab;
        private SerializedProperty _UsePooling;
        private SerializedProperty _DefaultPoolSize;
        private SerializedProperty _MaxPoolSize;

        private void OnEnable()
        {
            _Prefab = serializedObject.FindProperty("prefab");
            _UsePooling = serializedObject.FindProperty("_UsePooling");
            _DefaultPoolSize = serializedObject.FindProperty("_DefaultPoolSize");
            _MaxPoolSize = serializedObject.FindProperty("_MaxPoolSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_Prefab != null)
                EditorGUILayout.PropertyField(_Prefab);

            if (_UsePooling != null)
            {
                EditorGUILayout.PropertyField(_UsePooling);

                if (_UsePooling.boolValue)
                {
                    EditorGUI.indentLevel++;
                    if (_DefaultPoolSize != null)
                        EditorGUILayout.PropertyField(_DefaultPoolSize);
                    if (_MaxPoolSize != null)
                        EditorGUILayout.PropertyField(_MaxPoolSize);
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
