using KnifeToss.Infrastructure.Persistence;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Represents the PlayerStatsEditorWindow class.
/// </summary>
public class PlayerStatsEditorWindow : EditorWindow
{
    private const string Title = "Player Stats";

    private string highestLevelKey = PlayerPrefsProgressStore.DefaultHighestLevelKey;
    private int fallbackLevel = 1;
    private int highestLevel;
    private bool hasExistingValue;

    /// <summary>
    /// Performs the ShowWindow operation.
    /// </summary>
    [MenuItem("Knife Toss/Player Stats")]
    public static void ShowWindow()
    {
        GetWindow<PlayerStatsEditorWindow>(Title);
    }

    /// <summary>
    /// Performs the OnEnable operation.
    /// </summary>
    private void OnEnable()
    {
        LoadFromPrefs();
    }

    /// <summary>
    /// Performs the OnGUI operation.
    /// </summary>
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

    /// <summary>
    /// Performs the LoadFromPrefs operation.
    /// </summary>
    private void LoadFromPrefs()
    {
        PlayerPrefsProgressStore store = new PlayerPrefsProgressStore(highestLevelKey);
        hasExistingValue = PlayerPrefs.HasKey(highestLevelKey);
        highestLevel = store.LoadHighestLevel(fallbackLevel);
        Repaint();
    }

    /// <summary>
    /// Performs the SaveToPrefs operation.
    /// </summary>
    private void SaveToPrefs(int level)
    {
        PlayerPrefsProgressStore store = new PlayerPrefsProgressStore(highestLevelKey);
        store.SaveHighestLevel(Mathf.Max(1, level));
        hasExistingValue = true;
        highestLevel = level;
    }

    /// <summary>
    /// Performs the ClearPrefs operation.
    /// </summary>
    private void ClearPrefs()
    {
        PlayerPrefs.DeleteKey(highestLevelKey);
        PlayerPrefs.Save();
        hasExistingValue = false;
        highestLevel = fallbackLevel;
    }
}
