using KnifeToss.Common.State;
using KnifeToss.Level.Data;

namespace KnifeToss.Level.States
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
