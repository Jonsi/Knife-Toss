using Game.Level.Configurations.Stage;
using Game.Level.Data;

namespace Game.Level.Interfaces
{
    /// <summary>
    /// Interface for accessing stage and level structure information.
    /// </summary>
    public interface ILevelCatalog
    {
        /// <summary>
        /// Gets the total number of stages.
        /// </summary>
        int StageCount { get; }

        /// <summary>
        /// Gets the number of levels in the specified stage.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <returns>The number of levels.</returns>
        int GetLevelCount(int stageIndex);

        /// <summary>
        /// Retrieves the configuration for a specific level.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <param name="levelIndex">The index of the level within the stage.</param>
        /// <returns>The level configuration.</returns>
        LevelConfig GetLevelConfig(int stageIndex, int levelIndex);

        /// <summary>
        /// Retrieves the configuration object for a given stage.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <returns>The stage configuration or null if unavailable.</returns>
        StageConfig GetStageConfig(int stageIndex);
    }
}
