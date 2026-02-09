#if TEST_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Base class for custom debug tabs. Extend this to create a new tab.
    /// </summary>
    public abstract class DebugTab
    {
        public abstract string Key { get; }
        public abstract string Title { get; }
        public virtual string IconStyleKey => "Icon_Settings";
        public virtual int SortIndex => 100;

        internal Action RefreshCallback { get; set; }

        /// <summary>
        /// Resolves the current scene's container for debug tab resolution.
        /// </summary>
        protected static IObjectResolver CurrentResolver
        {
            get
            {
                var scopes = UnityEngine.Object.FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None);
                foreach (var scope in scopes)
                {
                    if (scope.gameObject.scene.name != "DontDestroyOnLoad" && scope.Container != null)
                        return scope.Container;
                }
                return scopes.FirstOrDefault(s => s.Container != null)?.Container;
            }
        }

        /// <summary>
        /// Tries to resolve a service from the current container.
        /// </summary>
        protected static T TryResolve<T>(IObjectResolver resolver) where T : class
        {
            return resolver?.Resolve<T>();
        }

        /// <summary>
        /// Returns the first currency type id from the catalog, or empty string if none.
        /// </summary>
        protected static string GetFirstCurrencyId()
        {
            var catalog = TryResolve<Game.Economy.Data.CurrencyTypeCatalog>(CurrentResolver);
            var configs = catalog?.GetAllConfigs();
            if (configs == null || configs.Count == 0) return string.Empty;
            var first = configs[0];
            return first != null ? (first.TypeId ?? string.Empty) : string.Empty;
        }

        /// <summary>
        /// Returns sections of data to display in the tab.
        /// </summary>
        public abstract IEnumerable<DebugTabSection> GetSections();

        /// <summary>
        /// Call this to refresh the tab UI after changing values.
        /// </summary>
        protected void RequestRefresh()
        {
            RefreshCallback?.Invoke();
        }
    }

    /// <summary>
    /// A section within a debug tab containing entries and actions.
    /// </summary>
    public class DebugTabSection
    {
        public string Title { get; }
        public List<DebugTabEntry> Entries { get; }
        public List<DebugTabAction> Actions { get; }

        public DebugTabSection(string title)
        {
            Title = title;
            Entries = new List<DebugTabEntry>();
            Actions = new List<DebugTabAction>();
        }

        public DebugTabSection Add(string label, object value)
        {
            Entries.Add(new DebugTabEntry(label, value));
            return this;
        }

        public DebugTabSection AddAction(string label, Action callback)
        {
            Actions.Add(new DebugTabAction(label, callback));
            return this;
        }
    }

    /// <summary>
    /// A single label-value entry in a debug tab section.
    /// </summary>
    public class DebugTabEntry
    {
        public string Label { get; }
        public object Value { get; }

        public DebugTabEntry(string label, object value)
        {
            Label = label;
            Value = value;
        }
    }

    /// <summary>
    /// An action button in a debug tab section.
    /// </summary>
    public class DebugTabAction
    {
        public string Label { get; }
        public Action Callback { get; }

        public DebugTabAction(string label, Action callback)
        {
            Label = label;
            Callback = callback;
        }
    }
}
#endif
