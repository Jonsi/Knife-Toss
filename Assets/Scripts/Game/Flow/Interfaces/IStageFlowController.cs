namespace Game.Flow.Interfaces
{
    /// <summary>
    /// Interface for controlling the stage flow downward from the Game layer.
    /// </summary>
    public interface IStageFlowController
    {
        /// <summary>
        /// Starts the stage at the given collection and local stage indices.
        /// </summary>
        /// <param name="collectionIndex">World (stage collection) index.</param>
        /// <param name="stageIndex">Stage index within that collection.</param>
        void LoadStage(int collectionIndex, int stageIndex);
    }
}
