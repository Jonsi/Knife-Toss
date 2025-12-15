using System;
using System.Collections.Generic;
using Game.Entities.Target.Data.Behaviors;
using Game.Entities.Target.Views;
using UnityEngine;

namespace Game.Entities.Target.Data
{
    /// <summary>
    /// Configuration data for a single target instance in a level.
    /// </summary>
    [Serializable]
    public class TargetConfig : ISerializationCallbackReceiver
    {
        [field: Header("Visuals")]
        [field: Tooltip("Optional override for the target prefab. If null, default is used.")]
        [field: SerializeField] public TargetView PrefabOverride { get; private set; }

        [field: Tooltip("Base scale multiplier for the target.")]
        [field: SerializeField] public float BaseScale { get; private set; } = 1f;

        [field: Header("Gameplay")]
        [field: Tooltip("Number of knife hits required to destroy this target.")]
        [field: SerializeField] public int KnivesToBreak { get; private set; } = 5;

        [field: Tooltip("Initial local rotation offset in degrees.")]
        [field: SerializeField] public float InitialRotation { get; private set; }

        [field: Header("Behaviors")]
        [field: Tooltip("Local and world behaviors applied to this target.")]
        [field: SerializeField] public TargetBehaviorSet Behaviors { get; private set; } = new();

        [field: Header("Collectibles")]
        [field: Tooltip("List of collectibles to attach to this target.")]
        [field: SerializeField] public List<Game.Entities.Collectible.Data.CollectibleConfig> Collectibles { get; private set; } = new();

        [field: Header("Pre-Stuck Knives")]
        [field: Tooltip("Knives that should start already stuck to this target.")]
        [field: SerializeField] public List<PreStuckKnifeConfig> PreStuckKnives { get; private set; } = new();

        /// <inheritdoc />
        public void OnBeforeSerialize()
        {
            Behaviors ??= new TargetBehaviorSet();
        }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            Behaviors ??= new TargetBehaviorSet();
            Collectibles ??= new List<Game.Entities.Collectible.Data.CollectibleConfig>();
            PreStuckKnives ??= new List<PreStuckKnifeConfig>();
        }
    }
}
