using System;
using System.Collections.Generic;
using KnifeToss.Core.Attributes;
using KnifeToss.Core.Data.Target;
using UnityEngine;

namespace KnifeToss.Core.Data.Level
{
    /// <summary>
    /// Configuration for a single game level.
    /// </summary>
    [Serializable]
    public class LevelConfig
    {
        [Tooltip("Total calculated knives for the level (Sum of Targets + Extra).")]
        [ReadOnly]
        [field: SerializeField] public int KnifeCount { get; set; }

        [Tooltip("Additional margin of knives allowed beyond the target requirements.")]
        [field: SerializeField] public int ExtraKnives { get; private set; } = 3;
        
        [Tooltip("List of targets to spawn in this level.")]
        [field: SerializeField] public List<TargetConfig> Targets { get; private set; } = new();
    }
}
