using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace KnifeToss.Core.Abstractions.Factories
{
    /// <summary>
    /// Factory interface for creating and recycling knife instances.
    /// </summary>
    public interface IKnifeFactory
    {
        /// <summary>
        /// Creates a new knife instance from the pool.
        /// </summary>
        Knife Create();

        /// <summary>
        /// Returns a knife instance to the pool for reuse.
        /// </summary>
        void Recycle(Knife knife);
    }
}

