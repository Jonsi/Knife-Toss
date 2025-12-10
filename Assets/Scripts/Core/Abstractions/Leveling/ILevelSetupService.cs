namespace KnifeToss.Core.Abstractions.Leveling
{
    /// <summary>
    /// Contract for loading levels.
    /// </summary>
    public interface ILevelSetupService
    {
        /// <summary>
        /// Loads the specified level and starts the session.
        /// </summary>
        /// <param name="stageIndex">The stage index.</param>
        /// <param name="levelIndex">The level index.</param>
        void LoadLevel(int stageIndex, int levelIndex);
    }
}

