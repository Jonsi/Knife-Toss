using System;
using System.Collections.Generic;
using Common.Attributes;
using Game.Entities.Target.Data;
using UnityEngine;

namespace Game.Level.Data
{
    /// <summary>
    /// Configuration for a single game level.
    /// </summary>
    [Serializable]
    public class LevelConfig
    {
        [Tooltip("Total calculated knives for the level (sum of target requirements + extra knives).")]
        [ReadOnly]
        [field: SerializeField] public int KnifeCount { get; set; }

        [Tooltip("Additional knives to grant the player for this level.")]
        [field: SerializeField] public int ExtraKnives { get; set; }

        [Tooltip("List of targets to spawn in this level.")]
        [field: SerializeField] public List<TargetConfig> Targets { get; private set; } = new();
    }
}
