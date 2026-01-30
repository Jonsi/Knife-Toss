#if TEST_ENABLED
using System.Text;
using SRDebugger.Services;
using SRDebugger.UI.Controls;
using SRDebugger.UI.Other;
using SRF.Service;
using UnityEngine;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Loads custom tabs from the registry when SRDebugger panel opens.
    /// </summary>
    public static class DebugTabLoader
    {
        private const string LabelColor = "#BCBCBC";
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private static bool _Initialized;
        private static bool _TabsCreated;
        private static GameObject _BaseTabPrefab;
        private static InfoBlock _InfoBlockPrefab;

        public static void Initialize()
        {
            if (_Initialized) return;
            _Initialized = true;

            _BaseTabPrefab = Resources.Load<GameObject>("Debug/CustomTab");

            if (_BaseTabPrefab == null)
            {
                Debug.LogError("[DebugTabLoader] CustomTab prefab not found at Resources/Debug/CustomTab. See setup instructions.");
                return;
            }

            var panelService = SRServiceManager.GetService<IDebugPanelService>();
            panelService.VisibilityChanged += OnVisibilityChanged;
        }

        private static void OnVisibilityChanged(IDebugPanelService service, bool visible)
        {
            if (!visible || _TabsCreated) return;
            _TabsCreated = true;

            var impl = service as SRDebugger.Services.Implementation.DebugPanelServiceImpl;
            if (impl?.RootObject == null) return;

            var tabController = impl.RootObject.TabController?.TabController;
            if (tabController == null) return;

            _InfoBlockPrefab = Resources.Load<InfoBlock>("Debug/InfoBlock");

            if (_InfoBlockPrefab == null)
            {
                Debug.LogError("[DebugTabLoader] InfoBlock prefab not found. Copy Assets/StompyRobot/SRDebugger/UI/Prefabs/InfoBlock.prefab to Assets/Resources/Debug/InfoBlock.prefab");
                return;
            }

            foreach (var tab in DebugTabRegistry.Tabs)
            {
                CreateTab(tabController, tab);
            }
        }

        private static void CreateTab(SRTabController tabController, DebugTab debugTab)
        {
            var instance = Object.Instantiate(_BaseTabPrefab);
            instance.name = $"Tab_{debugTab.Key}";

            var srTab = instance.GetComponent<SRTab>();
            SetTabProperties(srTab, debugTab);

            var controller = instance.GetComponent<CustomTabController>();
            if (controller != null)
            {
                controller.Setup(debugTab, _InfoBlockPrefab);
            }

            tabController.AddTab(srTab);
        }

        private static void SetTabProperties(SRTab srTab, DebugTab debugTab)
        {
            var type = typeof(SRTab);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            type.GetField("_title", flags)?.SetValue(srTab, debugTab.Title);
            type.GetField("_longTitle", flags)?.SetValue(srTab, debugTab.Title);
            type.GetField("_key", flags)?.SetValue(srTab, debugTab.Key);

            srTab.IconStyleKey = debugTab.IconStyleKey;
            srTab.SortIndex = debugTab.SortIndex;
        }

        public static string FormatSection(DebugTabSection section)
        {
            StringBuilder.Clear();

            var maxLen = 0;
            foreach (var entry in section.Entries)
            {
                if (entry.Label.Length > maxLen) maxLen = entry.Label.Length;
            }
            maxLen += 2;

            var first = true;
            foreach (var entry in section.Entries)
            {
                if (!first) StringBuilder.AppendLine();
                first = false;

                StringBuilder.Append($"<color={LabelColor}>{entry.Label}: </color>");
                for (var i = entry.Label.Length; i <= maxLen; i++) StringBuilder.Append(' ');
                StringBuilder.Append(entry.Value);
            }

            return StringBuilder.ToString();
        }
    }
}
#endif
