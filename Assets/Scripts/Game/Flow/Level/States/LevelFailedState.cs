using Game.App.Flow.Events;

namespace Game.Flow.Level.States
{
    public class LevelFailedState : LevelStateBase 
    {
        public LevelFailedState(LevelStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            // Notify upward flow
            Machine.NotifyLevelFailed();
        }
    }
}

