using KnifeToss.Core.Abstractions.State;

namespace KnifeToss.Core.Domain.Level
{
    /// <summary>
    /// Base class for level state implementations.
    /// </summary>
    public abstract class LevelStateBase : IState
    {
        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
    }
}

