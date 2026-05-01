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
        [Header("Theme")]
        [field: SerializeField]
        [field: Tooltip("Display name for this world in menus.")]
        public string ThemeName { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Background image for this world's row in the stage select screen.")]
        public Sprite StageSelectBackground { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Particle prefab for the main menu when this world is active.")]
        public GameObject MenuParticleEffectPrefab { get; private set; }

        [Header("Default Prefabs")]
        [field: SerializeField] public TargetView DefaultTargetPrefab { get; private set; }
        
        [Tooltip("List of all game stages.")]
        [field: SerializeField] public List<StageConfig> Stages { get; set; } = new();
    }
}

