using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnifeToss.Game.Level.Data
{
    /// <summary>
    /// Configuration for a game stage, containing visual settings and a sequence of levels.
    /// </summary>
    [Serializable]
    public class StageConfig
    {
        [Tooltip("Display name of the stage.")]
        [field: SerializeField] public string StageName { get; private set; }
        
        [Tooltip("Visual layout settings for this stage.")]
        [field: SerializeField] public StageLayoutSettings LayoutSettings { get; private set; }
        
        [Tooltip("List of levels contained in this stage.")]
        [field: SerializeField] public List<LevelConfig> Levels { get; private set; } = new();
    }
}

