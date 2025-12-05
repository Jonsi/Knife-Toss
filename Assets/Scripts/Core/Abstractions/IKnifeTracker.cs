using System.Collections.Generic;
using Presentation.Gameplay.Projectiles;

namespace Core.Abstractions
{
    /// <summary>
    /// Defines the IKnifeTracker interface.
    /// </summary>
    public interface IKnifeTracker
    {
        /// <summary>
        /// Performs the Register operation.
        /// </summary>
        void Register(Knife knife);
        /// <summary>
        /// Performs the Unregister operation.
        /// </summary>
        void Unregister(Knife knife);
        IEnumerable<Knife> Snapshot();
    }
}
