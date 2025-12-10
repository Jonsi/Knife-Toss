namespace Game.Flow.Interfaces
{
    /// <summary>
    /// Interface for controlling the stage flow downward from the Game layer.
    /// </summary>
    public interface IStageFlowController
    {
        /// <summary>
        /// Starts the stage at the specified index.
        /// </summary>
        void LoadStage(int stageIndex);
    }
}
