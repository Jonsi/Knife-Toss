using Core.Abstractions;
using Core.Domain;

namespace Presentation.UI.ViewModels
{



    public readonly struct LevelHudViewModel
    {
        public int StageIndex { get; }
        public int LevelIndex { get; }
        public string LevelLabel { get; }
        public string KnivesRemainingLabel { get; }
        public string LevelCompleteLabel { get; }
        public int TotalKnives { get; }
        public int KnivesThrown { get; }
        public int KnivesRemaining => UnityEngine.Mathf.Max(0, TotalKnives - KnivesThrown);
        public int FruitsHit { get; }
        public int TotalFruitsHit { get; }

        /// <summary>
        /// Performs the LevelHudViewModel operation.
        /// </summary>
        private LevelHudViewModel(
            int stageIndex,
            int levelIndex,
            string levelLabel,
            string knivesRemainingLabel,
            string levelCompleteLabel,
            int totalKnives,
            int knivesThrown,
            int fruitsHit,
            int totalFruitsHit)
        {
            StageIndex = stageIndex;
            LevelIndex = levelIndex;
            LevelLabel = levelLabel;
            KnivesRemainingLabel = knivesRemainingLabel;
            LevelCompleteLabel = levelCompleteLabel;
            TotalKnives = totalKnives;
            KnivesThrown = knivesThrown;
            FruitsHit = fruitsHit;
            TotalFruitsHit = totalFruitsHit;
        }

        /// <summary>
        /// Performs the FromSession operation.
        /// </summary>
        public static LevelHudViewModel FromSession(LevelSession session, IPlayerDataService playerDataService = null)
        {
            if (session?.Rules == null || session.Rules.KnifeCount <= 0)
            {
                int totalFruits = playerDataService?.GetTotalFruitsHit() ?? 0;
                return new LevelHudViewModel(0, 0, string.Empty, string.Empty, string.Empty, 0, 0, 0, totalFruits);
            }

            int stageIndex = session.StageIndex;
            int levelIndex = session.LevelIndex;
            int total = UnityEngine.Mathf.Max(0, session.Rules.KnifeCount);
            int thrown = UnityEngine.Mathf.Clamp(session.KnivesThrown, 0, total);
            int remainingLabelValue = UnityEngine.Mathf.Max(0, session.Rules.KnifeCount - session.PlayerKnivesStuck);
            string levelLabel = $"{stageIndex}-{levelIndex}";
            string completeLabel = $"STAGE {stageIndex} - LEVEL {levelIndex}";
            int fruitsHit = session.HitableObjectsHit;
            int totalFruitsHit = playerDataService?.GetTotalFruitsHit() ?? 0;
            return new LevelHudViewModel(stageIndex, levelIndex, levelLabel, remainingLabelValue.ToString(), completeLabel, total, thrown, fruitsHit, totalFruitsHit);
        }
    }
}
