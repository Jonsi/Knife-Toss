using System;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Interface for listening to player throw input actions and notifying subscribers when a throw is requested.
    /// </summary>
    public interface IPlayerThrowInputListener
    {
        /// <summary>
        /// Event raised when the player starts a throw action (e.g., press).
        /// </summary>
        event Action OnThrowRequested;

        /// <summary>
        /// Event raised when the player completes a throw action (e.g., release).
        /// </summary>
        event Action OnThrowReleased;
        
        /// <summary>
        /// Initializes the input listener and begins listening for player throw input.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Disposes of resources and stops listening for input.
        /// </summary>
        void Dispose();
    }
}

