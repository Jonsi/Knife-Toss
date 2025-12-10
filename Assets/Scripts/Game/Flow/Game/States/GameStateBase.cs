using Common.Patterns.State;

namespace Game.Flow.Game.States
{
    public abstract class GameStateBase : IState
    {
        protected readonly GameStateMachine Machine;
        protected GameData Context => Machine.Context;

        protected GameStateBase(GameStateMachine machine)
        {
            Machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
    }
}
