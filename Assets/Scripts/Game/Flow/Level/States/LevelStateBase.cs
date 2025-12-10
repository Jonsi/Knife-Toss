using Common.Patterns.State;

namespace Game.Flow.Level.States
{
    public abstract class LevelStateBase : IState
    {
        protected readonly LevelStateMachine Machine;
        protected LevelData Context => Machine.Context;

        protected LevelStateBase(LevelStateMachine machine)
        {
            Machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
    }
}

