using Game.App.Flow.Events;

namespace Game.Flow.Level.States
{
    public class LevelCompletedState : LevelStateBase 
    {
        public LevelCompletedState(LevelStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            // Notify upward flow
            Machine.NotifyLevelCompleted();
        }
    }
}

