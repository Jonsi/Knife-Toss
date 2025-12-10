using System.Collections.Generic;
using Game.Entities.Target.Views;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Master configuration asset holding all stages and levels.
    /// </summary>
    [CreateAssetMenu(fileName = "StagesConfig", menuName = "Knife Toss/Gameplay/Stages Config")]
    public class StagesConfigAsset : ScriptableObject
    {
        [Header("Default Prefabs")]
        [field: SerializeField] public TargetView DefaultTargetPrefab { get; private set; }
        
        [Tooltip("List of all game stages.")]
        [field: SerializeField] public List<StageConfig> Stages { get; private set; } = new();

        private void OnValidate()
        {
            if (Stages == null) return;

            foreach (var stage in Stages)
            {
                if (stage?.Levels == null) continue;
                foreach (var level in stage.Levels)
                {
                    if (level == null) continue;

                    int required = 0;
                    if (level.Targets != null)
                    {
                        foreach (var target in level.Targets)
                        {
                            if (target != null)
                                required += target.KnivesToBreak;
                        }
                    }
                    level.KnifeCount = required + level.ExtraKnives;
                }
            }
        }
    }
}
