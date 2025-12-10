namespace KnifeToss.Core.Abstractions.Flow
{
    /// <summary>
    /// Provides navigation logic to change level progression.
    /// </summary>
    public interface IStageNavigator
    {
        /// <summary>
        /// Moves to the next level in the current stage, or the first level of the next stage.
        /// </summary>
        void AdvanceLevel();

        /// <summary>
        /// Prepares to retry the current level.
        /// </summary>
        void RetryLevel();

        /// <summary>
        /// Resets progress to the start of the current stage.
        /// </summary>
        void RestartStage();
    }
}

