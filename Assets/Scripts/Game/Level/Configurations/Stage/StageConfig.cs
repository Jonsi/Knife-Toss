using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Configurations.Stage
{
    /// <summary>
    /// Configuration asset for a single game stage.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage", menuName = "Knife Toss/Gameplay/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        [Tooltip("Display name of the stage.")]
        [field: SerializeField] public string StageName { get; private set; }
        
        [Tooltip("Visual layout settings for this stage.")]
        [field: SerializeField] public StageLayoutSettings LayoutSettings { get; private set; }
        
        [Tooltip("List of levels contained in this stage.")]
        [field: SerializeField] public List<LevelConfig> Levels { get; private set; } = new();

        private void OnValidate()
        {
            if (Levels == null) return;

            foreach (var level in Levels)
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
