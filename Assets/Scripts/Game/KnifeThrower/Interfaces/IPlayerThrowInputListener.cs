using System;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Interface for listening to player throw input actions and notifying subscribers when a throw is requested.
    /// </summary>
    public interface IPlayerThrowInputListener
    {
        /// <summary>
        /// Event raised when the player requests a knife throw.
        /// </summary>
        event Action OnThrowRequested;
        
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

