namespace KnifeToss.Game.Knife.Interfaces
{
    /// <summary>
    /// Interface for controlling knife behavior and state transitions.
    /// </summary>
    public interface IKnifeController
    {
        /// <summary>
        /// Launches the knife with the specified speed.
        /// </summary>
        /// <param name="speed">The speed at which to launch the knife.</param>
        void Launch(float speed);
    }
}

