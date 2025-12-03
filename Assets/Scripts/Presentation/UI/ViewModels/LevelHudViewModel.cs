using KnifeToss.Core.Domain;

namespace KnifeToss.Presentation.UI
{



    public readonly struct LevelHudViewModel
    {
        public string LevelLabel { get; }
        public string KnivesRemainingLabel { get; }
        public string LevelCompleteLabel { get; }

        /// <summary>
        /// Performs the LevelHudViewModel operation.
        /// </summary>
        private LevelHudViewModel(string levelLabel, string knivesRemainingLabel, string levelCompleteLabel)
        {
            LevelLabel = levelLabel;
            KnivesRemainingLabel = knivesRemainingLabel;
            LevelCompleteLabel = levelCompleteLabel;
        }

        /// <summary>
        /// Performs the FromSession operation.
        /// </summary>
        public static LevelHudViewModel FromSession(LevelSession session)
        {
            if (session == null || session.Rules.KnifeCount <= 0)
            {
                return new LevelHudViewModel(string.Empty, string.Empty, string.Empty);
            }

            int remaining = UnityEngine.Mathf.Max(0, session.Rules.KnifeCount - session.PlayerKnivesStuck);
            string levelLabel = $"{session.LevelIndex}";
            string completeLabel = $"LEVEL {levelLabel}";
            return new LevelHudViewModel(levelLabel, remaining.ToString(), completeLabel);
        }
    }
}
