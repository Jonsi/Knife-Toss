using Common.Patterns.State;

namespace Game.Flow.Stage.States
{
    public abstract class StageStateBase : IState
    {
        protected readonly StageStateMachine Machine;
        protected StageData Context => Machine.Context;

        protected StageStateBase(StageStateMachine machine)
        {
            Machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
    }
}
