using Game.Level.Configurations.Stage;
using Game.Level.Data;

namespace Game.Flow.Interfaces
{
    /// <summary>
    /// Interface for controlling the level flow downward from the Stage layer.
    /// </summary>
    public interface ILevelFlowController
    {
        /// <summary>
        /// Loads and starts the level using the provided configuration.
        /// </summary>
        /// <param name="stageConfig">Stage configuration that owns the level.</param>
        /// <param name="config">Specific level configuration to load.</param>
        void LoadLevel(StageConfig stageConfig, LevelConfig config);

        /// <summary>
        /// Retries the currently active level.
        /// </summary>
        void RetryLevel();
    }
}
