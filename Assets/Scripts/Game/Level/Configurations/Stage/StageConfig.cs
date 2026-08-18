using System.Collections.Generic;
using Game.Level.Data;
using Game.Rewards.Configs;
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

        [field: SerializeField]
        [field: Tooltip("Granted once when the player completes this stage for the first time. Each slot references a currency, power-up, or knife skin grant asset.")]
        public List<RewardGrantEntryBase> Rewards { get; private set; } = new();

        [field: SerializeField]
        [field: Tooltip("Coin cost to continue after failing each level in this stage. Index 0 is Level 1.")]
        public List<int> ContinueCosts { get; private set; } = new() { 50 };
        
        [Tooltip("List of levels contained in this stage.")]
        [field: SerializeField] public List<LevelConfig> Levels { get; private set; } = new();

        public int GetContinueCost(int levelIndex)
        {
            if (ContinueCosts == null || levelIndex < 0 || levelIndex >= ContinueCosts.Count)
            {
                return 0;
            }

            return Mathf.Max(0, ContinueCosts[levelIndex]);
        }

        private void OnValidate()
        {
            if (ContinueCosts == null)
            {
                ContinueCosts = new List<int>();
            }

            if (Levels != null)
            {
                while (ContinueCosts.Count < Levels.Count)
                {
                    int nextCost = ContinueCosts.Count > 0 ? ContinueCosts[^1] + 50 : 50;
                    ContinueCosts.Add(nextCost);
                }

                while (ContinueCosts.Count > Levels.Count)
                {
                    ContinueCosts.RemoveAt(ContinueCosts.Count - 1);
                }
            }

            for (int i = 0; i < ContinueCosts.Count; i++)
            {
                ContinueCosts[i] = Mathf.Max(1, ContinueCosts[i]);
            }

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
