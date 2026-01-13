using System.Collections.Generic;
using Game.Entities.Target.Views;
using Game.Level.Configurations.Stage;
using UnityEngine;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Master configuration asset holding all stages and levels.
    /// </summary>
    [CreateAssetMenu(fileName = "StagesCollection", menuName = "Knife Toss/Gameplay/Stages Collection")]
    public class StagesCollection : ScriptableObject
    {
        [Header("Default Prefabs")]
        [field: SerializeField] public TargetView DefaultTargetPrefab { get; private set; }
        
        [Tooltip("List of all game stages.")]
        [field: SerializeField] public List<StageConfig> Stages { get; set; } = new();
    }
}

