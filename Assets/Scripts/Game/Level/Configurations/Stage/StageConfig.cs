using System;
using System.Collections.Generic;
using Game.Entities.Collectible.Views;
using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.Level.Data
{
    /// <summary>
    /// Configuration for a single game stage, containing visual settings and a sequence of levels.
    /// </summary>
    [Serializable]
    public class StageConfig
    {
        [Tooltip("Display name of the stage.")]
        [field: SerializeField] public string StageName { get; private set; }
        
        [Header("Default Prefabs")]
        [Tooltip("Default knife prefab used for any pre-stuck knives in this stage.")]
        [field: SerializeField] public KnifeView DefaultKnifePrefab { get; private set; }

        [Tooltip("Default collectible prefab used when spawning collectibles in this stage.")]
        [field: SerializeField] public CollectibleView DefaultCollectiblePrefab { get; private set; }
        
        [Tooltip("Visual layout settings for this stage.")]
        [field: SerializeField] public StageLayoutSettings LayoutSettings { get; private set; }
        
        [Tooltip("List of levels contained in this stage.")]
        [field: SerializeField] public List<LevelConfig> Levels { get; private set; } = new();
    }
}
