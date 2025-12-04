using KnifeToss.Core.Domain;

namespace KnifeToss.Presentation.UI
{



    public readonly struct LevelHudViewModel
    {
        public string LevelLabel { get; }
        public string KnivesRemainingLabel { get; }
        public string LevelCompleteLabel { get; }
        public int TotalKnives { get; }
        public int KnivesThrown { get; }
        public int KnivesRemaining => UnityEngine.Mathf.Max(0, TotalKnives - KnivesThrown);

        /// <summary>
        /// Performs the LevelHudViewModel operation.
        /// </summary>
        private LevelHudViewModel(
            string levelLabel,
            string knivesRemainingLabel,
            string levelCompleteLabel,
            int totalKnives,
            int knivesThrown)
        {
            LevelLabel = levelLabel;
            KnivesRemainingLabel = knivesRemainingLabel;
            LevelCompleteLabel = levelCompleteLabel;
            TotalKnives = totalKnives;
            KnivesThrown = knivesThrown;
        }

        /// <summary>
        /// Performs the FromSession operation.
        /// </summary>
        public static LevelHudViewModel FromSession(LevelSession session)
        {
            if (session?.Rules == null || session.Rules.KnifeCount <= 0)
            {
                return new LevelHudViewModel(string.Empty, string.Empty, string.Empty, 0, 0);
            }

            int total = UnityEngine.Mathf.Max(0, session.Rules.KnifeCount);
            int thrown = UnityEngine.Mathf.Clamp(session.KnivesThrown, 0, total);
            int remainingLabelValue = UnityEngine.Mathf.Max(0, session.Rules.KnifeCount - session.PlayerKnivesStuck);
            string levelLabel = $"{session.LevelIndex}";
            string completeLabel = $"LEVEL {levelLabel}";
            return new LevelHudViewModel(levelLabel, remainingLabelValue.ToString(), completeLabel, total, thrown);
        }
    }
}
