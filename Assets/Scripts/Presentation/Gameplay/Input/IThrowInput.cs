using System;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Defines the IThrowInput interface.
    /// </summary>
    public interface IThrowInput
    {
        event Action<Vector2> Pressed;
        event Action<Vector2> Released;
        Vector2 CurrentScreenPosition { get; }
        /// <summary>
        /// Performs the Enable operation.
        /// </summary>
        void Enable();
        /// <summary>
        /// Performs the Disable operation.
        /// </summary>
        void Disable();
    }
}
