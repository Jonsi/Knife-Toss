using System.Collections.Generic;
using System.Linq;
using KnifeToss.Core.Abstractions;
using KnifeToss.Presentation.Gameplay.Projectiles;

namespace KnifeToss.Infrastructure.Leveling
{
    public sealed class KnifeTracker : IKnifeTracker
    {
        private readonly HashSet<Knife> knives = new HashSet<Knife>();

        public void Register(Knife knife)
        {
            if (knife != null)
            {
                knives.Add(knife);
            }
        }

        public void Unregister(Knife knife)
        {
            if (knife != null)
            {
                knives.Remove(knife);
            }
        }

        public IEnumerable<Knife> Snapshot()
        {
            return knives.ToArray();
        }
    }
}
