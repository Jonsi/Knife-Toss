using Game.Entities.Target.Logic;

namespace Game.Entities.Target.Interfaces
{
    /// <summary>
    /// Defines a world-space behavior that can reposition a target each frame.
    /// </summary>
    public interface IWorldTargetBehavior
    {
        /// <summary>
        /// Updates the behavior using the provided runtime context.
        /// </summary>
        /// <param name="runtime">Runtime data for the current frame.</param>
        void Tick(WorldBehaviorRuntime runtime);
    }
}

