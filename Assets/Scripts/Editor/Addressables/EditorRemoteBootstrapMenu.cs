using Game.Flow.Loading;
using UnityEditor;

namespace Editor.Addressables
{
    public static class EditorRemoteBootstrapMenu
    {
        private const string MenuPath = "Tools/Addressables/Force Remote Bootstrap In Play Mode";

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            EditorRemoteBootstrapSettings.Enabled = !EditorRemoteBootstrapSettings.Enabled;
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorRemoteBootstrapSettings.Enabled);
            return true;
        }
    }
}
