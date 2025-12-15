using Game.Entities.Target.Logic;

namespace Game.Entities.Target.Interfaces
{
    /// <summary>
    /// Defines a local-space behavior that can influence a target each frame.
    /// </summary>
    public interface ILocalTargetBehavior
    {
        /// <summary>
        /// Ticks the behavior using the provided runtime context.
        /// </summary>
        /// <param name="runtime">Runtime data for the current frame.</param>
        void Tick(LocalBehaviorRuntime runtime);
    }
}

