namespace KnifeToss.Core.Abstractions.Flow
{
    /// <summary>
    /// Tracks current progression state (stage/level indices).
    /// </summary>
    public interface IStageProgressTracker
    {
        int CurrentStageIndex { get; }
        int CurrentLevelIndex { get; }
    }
}
