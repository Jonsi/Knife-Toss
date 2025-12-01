using KnifeToss.Infrastructure.Persistence;
using UnityEditor;
using UnityEngine;

public class PlayerStatsEditorWindow : EditorWindow
{
    private const string Title = "Player Stats";

    private string highestLevelKey = PlayerPrefsProgressStore.DefaultHighestLevelKey;
    private int fallbackLevel = 1;
    private int highestLevel;
    private bool hasExistingValue;

    [MenuItem("Knife Toss/Player Stats")]
    public static void ShowWindow()
    {
        GetWindow<PlayerStatsEditorWindow>(Title);
    }

    private void OnEnable()
    {
        LoadFromPrefs();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Player Progress (PlayerPrefs)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        highestLevelKey = EditorGUILayout.TextField("Progress Key", highestLevelKey);
        fallbackLevel = EditorGUILayout.IntField("Fallback Level", Mathf.Max(1, fallbackLevel));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stored Highest Level", hasExistingValue ? highestLevel.ToString() : "No saved value");

        highestLevel = EditorGUILayout.IntField("Edit Highest Level", Mathf.Max(1, highestLevel));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
        {
            LoadFromPrefs();
        }

        if (GUILayout.Button("Save"))
        {
            SaveToPrefs(highestLevel);
        }

        if (GUILayout.Button("Clear"))
        {
            ClearPrefs();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void LoadFromPrefs()
    {
        PlayerPrefsProgressStore store = new PlayerPrefsProgressStore(highestLevelKey);
        hasExistingValue = PlayerPrefs.HasKey(highestLevelKey);
        highestLevel = store.LoadHighestLevel(fallbackLevel);
        Repaint();
    }

    private void SaveToPrefs(int level)
    {
        PlayerPrefsProgressStore store = new PlayerPrefsProgressStore(highestLevelKey);
        store.SaveHighestLevel(Mathf.Max(1, level));
        hasExistingValue = true;
        highestLevel = level;
    }

    private void ClearPrefs()
    {
        PlayerPrefs.DeleteKey(highestLevelKey);
        PlayerPrefs.Save();
        hasExistingValue = false;
        highestLevel = fallbackLevel;
    }
}
