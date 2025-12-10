namespace Game.Level.States
{
    /// <summary>
    /// Level has failed.
    /// Waits for player input to Retry or Restart.
    /// </summary>
    public class LevelFailedState : LevelStateBase 
    {
        public LevelFailedState(StageStateMachine machine) : base(machine) { }

        public void RetryLevel()
        {
            Machine.SetState(Machine.Loading);
        }

        public void RestartStage()
        {
            Context.CurrentLevelIndex = 0;
            Machine.SetState(Machine.Loading);
        }
    }
}
