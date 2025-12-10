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
            machine.SetState(machine.loading);
        }

        public void RestartStage()
        {
            Context.CurrentLevelIndex = 0;
            machine.SetState(machine.loading);
        }
    }
}
