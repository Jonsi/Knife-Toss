namespace Game.Flow.Stage.States
{
    public class StageCompletedState : StageStateBase
    {
        public StageCompletedState(StageStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            Machine.StageCompletedChannel.Raise(Context);
        }
    }
}

