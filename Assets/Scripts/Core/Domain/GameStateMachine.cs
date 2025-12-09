using KnifeToss.Core.Domain.Level;

namespace KnifeToss.Core.Domain
{
    /// <summary>
    /// State machine for level/game states; composes and owns level states.
    /// </summary>
    public class GameStateMachine : StateMachine
    {
        public readonly LevelLoadingState Loading;
        public readonly LevelPlayingState Playing;
        public readonly LevelCompletedState Completed;
        public readonly LevelFailedState Failed;

        public GameStateMachine(LevelLoadingState loading, LevelPlayingState playing, LevelCompletedState completed, LevelFailedState failed)
        {
            Loading = loading;
            Playing = playing;
            Completed = completed;
            Failed = failed;

            SetState(Loading);
        }

        public bool IsPlaying => ReferenceEquals(CurrentState, Playing);
    }
}
