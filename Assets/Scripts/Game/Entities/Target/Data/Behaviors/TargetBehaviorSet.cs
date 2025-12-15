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
        [UnityEngine.SerializeField] private List<TargetBehaviorEntry> _Behaviors = new();

        /// <summary>
        /// Gets the configured behaviors.
        /// </summary>
        public IReadOnlyList<TargetBehaviorEntry> Behaviors => _Behaviors;

        /// <summary>
        /// Gets a value indicating whether the set is empty.
        /// </summary>
        public bool IsEmpty => _Behaviors == null || _Behaviors.Count == 0;

        /// <summary>
        /// Adds a behavior configuration.
        /// </summary>
        /// <param name="config">Behavior configuration.</param>
        public void AddBehavior(TargetBehaviorConfig config)
        {
            if (config == null)
                return;

            EnsureList();
            _Behaviors.Add(TargetBehaviorEntry.From(config));
        }

        /// <summary>
        /// Creates runtime behaviors for this set.
        /// </summary>
        /// <returns>Runtime behavior collection.</returns>
        public TargetBehaviorCollection CreateRuntimeCollection()
        {
            var collection = new TargetBehaviorCollection();
            if (_Behaviors == null || _Behaviors.Count == 0)
                return collection;

            for (int i = 0; i < _Behaviors.Count; i++)
            {
                TargetBehaviorEntry entry = _Behaviors[i];
                if (entry == null || !entry.Enabled)
                    continue;

                if (entry.Config is IWorldBehaviorConfig worldConfig)
                {
                    var behavior = worldConfig.CreateWorldBehavior();
                    if (behavior != null)
                        collection.AddWorldBehavior(behavior);
                    continue;
                }

                if (entry.Config is ILocalBehaviorConfig localConfig)
                {
                    var behavior = localConfig.CreateLocalBehavior();
                    if (behavior != null)
                        collection.AddLocalBehavior(behavior);
                }
            }

            return collection;
        }

        void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_Behaviors == null)
                return;

            for (int i = 0; i < _Behaviors.Count; i++)
            {
                _Behaviors[i]?.Sync();
            }
        }

        void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_Behaviors == null)
                _Behaviors = new List<TargetBehaviorEntry>();

            for (int i = 0; i < _Behaviors.Count; i++)
            {
                _Behaviors[i]?.Sync();
            }
        }

        private void EnsureList()
        {
            _Behaviors ??= new List<TargetBehaviorEntry>();
        }
    }
}
