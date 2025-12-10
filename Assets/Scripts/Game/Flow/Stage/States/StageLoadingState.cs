using Game.Level.Data;

namespace Game.Flow.Stage.States
{
    public class StageLoadingState : StageStateBase
    {
        public StageLoadingState(StageStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            Context.CurrentLevelIndex = 0;
            LoadCurrentLevel();
        }

        private void LoadCurrentLevel()
        {
            LevelConfig config = Machine.LevelCatalog.GetLevelConfig(Context.StageIndex, Context.CurrentLevelIndex);
            Machine.LevelFlowController.LoadLevel(config);
            Machine.SetState(Machine.Playing);
        }
    }
}

