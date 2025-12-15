using System;
using System.Collections.Generic;
using Game.Entities.Target.Interfaces;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Container grouping all local and world behavior configurations.
    /// </summary>
    [Serializable]
    public class TargetBehaviorSet : UnityEngine.ISerializationCallbackReceiver
    {
        [UnityEngine.SerializeField] private List<TargetLocalBehaviorEntry> _LocalBehaviors = new();
        [UnityEngine.SerializeField] private List<TargetWorldBehaviorEntry> _WorldBehaviors = new();

        /// <summary>
        /// Gets the configured local behaviors.
        /// </summary>
        public IReadOnlyList<TargetLocalBehaviorEntry> LocalBehaviors => _LocalBehaviors;

        /// <summary>
        /// Gets the configured world behaviors.
        /// </summary>
        public IReadOnlyList<TargetWorldBehaviorEntry> WorldBehaviors => _WorldBehaviors;

        /// <summary>
        /// Gets a value indicating whether the set is empty.
        /// </summary>
        public bool IsEmpty => (_LocalBehaviors == null || _LocalBehaviors.Count == 0) &&
                               (_WorldBehaviors == null || _WorldBehaviors.Count == 0);

        /// <summary>
        /// Adds a behavior configuration.
        /// </summary>
        /// <param name="config">Behavior configuration.</param>
        public void AddBehavior(ILocalBehaviorConfig config)
        {
            if (config == null)
                return;

            EnsureLocalList();
            _LocalBehaviors.Add(TargetLocalBehaviorEntry.From(config));
        }

        /// <summary>
        /// Adds a world behavior configuration.
        /// </summary>
        /// <param name="config">Behavior configuration.</param>
        public void AddWorldBehavior(IWorldBehaviorConfig config)
        {
            if (config == null)
                return;

            EnsureWorldList();
            _WorldBehaviors.Add(TargetWorldBehaviorEntry.From(config));
        }

        /// <summary>
        /// Creates runtime behaviors for this set.
        /// </summary>
        /// <returns>Runtime behavior collection.</returns>
        public TargetBehaviorCollection CreateRuntimeCollection()
        {
            var collection = new TargetBehaviorCollection();
            AppendWorldBehaviors(collection, _WorldBehaviors);
            AppendLocalBehaviors(collection, _LocalBehaviors);
            return collection;
        }

        void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SyncEntries(_LocalBehaviors);
            SyncEntries(_WorldBehaviors);
        }

        void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _LocalBehaviors ??= new List<TargetLocalBehaviorEntry>();
            _WorldBehaviors ??= new List<TargetWorldBehaviorEntry>();
            SyncEntries(_LocalBehaviors);
            SyncEntries(_WorldBehaviors);
        }

        private static void SyncEntries(List<TargetLocalBehaviorEntry> entries)
        {
            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i]?.Sync();
            }
        }

        private static void SyncEntries(List<TargetWorldBehaviorEntry> entries)
        {
            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i]?.Sync();
            }
        }

        private static void AppendLocalBehaviors(TargetBehaviorCollection collection, List<TargetLocalBehaviorEntry> entries)
        {
            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                TargetLocalBehaviorEntry entry = entries[i];
                if (entry?.Config == null)
                    continue;

                var behavior = entry.Config.CreateLocalBehavior();
                if (behavior != null)
                    collection.AddLocalBehavior(behavior);
            }
        }

        private static void AppendWorldBehaviors(TargetBehaviorCollection collection, List<TargetWorldBehaviorEntry> entries)
        {
            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                TargetWorldBehaviorEntry entry = entries[i];
                if (entry?.Config == null)
                    continue;

                var behavior = entry.Config.CreateWorldBehavior();
                if (behavior != null)
                    collection.AddWorldBehavior(behavior);
            }
        }

        private void EnsureLocalList()
        {
            _LocalBehaviors ??= new List<TargetLocalBehaviorEntry>();
        }

        private void EnsureWorldList()
        {
            _WorldBehaviors ??= new List<TargetWorldBehaviorEntry>();
        }
    }
}
