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
        void LoadLevel(LevelConfig config);

        /// <summary>
        /// Retries the currently active level.
        /// </summary>
        void RetryLevel();
    }
}
