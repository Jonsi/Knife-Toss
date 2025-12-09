namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Strategy interface for target rotation behaviors.
    /// Allows interchangeable rotation algorithms at runtime.
    /// </summary>
    public interface IRotationBehavior
    {
        /// <summary>
        /// Calculates the rotation delta for the current frame.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        /// <returns>The rotation delta in degrees.</returns>
        float GetRotationDelta(float deltaTime);
    }
}

