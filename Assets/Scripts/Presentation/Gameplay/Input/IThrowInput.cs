using System;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Defines the IThrowInput interface.
    /// </summary>
    public interface IThrowInput
    {
        event Action Performed;
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
