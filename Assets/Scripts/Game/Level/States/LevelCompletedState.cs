namespace KnifeToss.Game.Level.States
{
    /// <summary>
    /// Level has met completion criteria.
    /// Advances progression and transitions to loading the next level.
    /// </summary>
    public class LevelCompletedState : LevelStateBase 
    {
        public LevelCompletedState(StageStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            int levelsInStage = Context.LevelCatalog.GetLevelCount(Context.CurrentStageIndex);
            
            if (Context.CurrentLevelIndex + 1 < levelsInStage)
            {
                Context.CurrentLevelIndex++;
            }
            else
            {
                if (Context.CurrentStageIndex + 1 < Context.LevelCatalog.StageCount)
                {
                    Context.CurrentStageIndex++;
                    Context.CurrentLevelIndex = 0;
                }
                else
                {
                    Context.CurrentStageIndex = 0;
                    Context.CurrentLevelIndex = 0;
                }
            }

            machine.SetState(machine.loading);
        }
    }
}
