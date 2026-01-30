#if TEST_ENABLED
using System;
using System.Collections.Generic;

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
