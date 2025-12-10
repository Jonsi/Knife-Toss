using KnifeToss.Core.Abstractions.State;

namespace KnifeToss.Core.Domain.Level
{
    /// <summary>
    /// Base class for level state implementations.
    /// Provides access to the StageStateMachine and StageData.
    /// </summary>
    public abstract class LevelStateBase : IState
    {
        protected readonly StageStateMachine Machine;
        protected StageData Context => Machine.Context;

        protected LevelStateBase(StageStateMachine machine)
        {
            Machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
    }
}
