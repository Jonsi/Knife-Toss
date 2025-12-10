using KnifeToss.Common.State;
using KnifeToss.Game.Level.Data;

namespace KnifeToss.Game.Level.States
{
    /// <summary>
    /// Base class for level state implementations.
    /// Provides access to the StageStateMachine and StageData.
    /// </summary>
    public abstract class LevelStateBase : IState
    {
        protected readonly StageStateMachine machine;
        protected StageData Context => machine.Context;

        protected LevelStateBase(StageStateMachine machine)
        {
            this.machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
    }
}
