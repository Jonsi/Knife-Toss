using Game.Flow.Game;
using Game.Flow.Level;
using Game.Flow.Stage;

namespace Game.UI.Interfaces
{
    /// <summary>
    /// Exposes runtime HUD values for the current stage and level.
    /// </summary>
    public interface ILevelHudData
    {
        /// <summary>
        /// Gets the one-based index of the current stage.
        /// </summary>
        int CurrentStage { get; }

        /// <summary>
        /// Gets the one-based index of the current level within the current stage.
        /// </summary>
        int CurrentLevel { get; }

        /// <summary>
        /// Gets the total number of levels in the current stage.
        /// </summary>
        int TotalLevelsInStage { get; }

        /// <summary>
        /// Gets the remaining knives available to the player in the current level.
        /// </summary>
        int KnivesRemaining { get; }

        /// <summary>
        /// Gets the total knives allocated for the current level.
        /// </summary>
        int TotalKnives { get; }

        /// <summary>
        /// Gets the total collectible currency accumulated in the session.
        /// </summary>
        int TotalCollectibles { get; }
    }
}
