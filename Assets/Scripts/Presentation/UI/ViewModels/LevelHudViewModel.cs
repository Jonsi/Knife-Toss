using KnifeToss.Core.Domain;

namespace KnifeToss.Presentation.UI
{
    /// <summary>
    /// Pure data for HUD binding.
    /// </summary>
    public readonly struct LevelHudViewModel
    {
        public string LevelLabel { get; }
        public string KnivesRemainingLabel { get; }

        private LevelHudViewModel(string levelLabel, string knivesRemainingLabel)
        {
            LevelLabel = levelLabel;
            KnivesRemainingLabel = knivesRemainingLabel;
        }

        public static LevelHudViewModel FromSession(LevelSession session)
        {
            if (session == null || session.Rules.RequiredKnives <= 0)
            {
                return new LevelHudViewModel(string.Empty, string.Empty);
            }

            int remaining = UnityEngine.Mathf.Max(0, session.Rules.RequiredKnives - session.KnivesStuck);
            return new LevelHudViewModel($"{session.LevelIndex}", remaining.ToString());
        }
    }
}
