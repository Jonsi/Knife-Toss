using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Presentation.Gameplay.Projectiles;

namespace Infrastructure.Leveling
{
    public sealed class KnifeTracker : IKnifeTracker
    {
        private readonly HashSet<Knife> knives = new HashSet<Knife>();

        /// <summary>
        /// Performs the Register operation.
        /// </summary>
        public void Register(Knife knife)
        {
            if (knife != null)
            {
                knives.Add(knife);
            }
        }

        /// <summary>
        /// Performs the Unregister operation.
        /// </summary>
        public void Unregister(Knife knife)
        {
            if (knife != null)
            {
                knives.Remove(knife);
            }
        }

        /// <summary>
        /// Performs the Snapshot operation.
        /// </summary>
        public IEnumerable<Knife> Snapshot()
        {
            return knives.ToArray();
        }
    }
}
