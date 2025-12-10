namespace KnifeToss.Core.Abstractions.Gameplay
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
    }
}
