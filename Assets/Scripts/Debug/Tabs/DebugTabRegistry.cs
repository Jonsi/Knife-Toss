#if TEST_ENABLED
using System.Collections.Generic;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Registry for custom debug tabs. Register tabs before SRDebugger panel opens.
    /// </summary>
    public static class DebugTabRegistry
    {
        private static readonly List<DebugTab> _Tabs = new List<DebugTab>();

        public static IReadOnlyList<DebugTab> Tabs => _Tabs;

        public static void Register(DebugTab tab)
        {
            if (tab == null) return;
            _Tabs.Add(tab);
        }

        public static void Clear()
        {
            _Tabs.Clear();
        }
    }
}
#endif
