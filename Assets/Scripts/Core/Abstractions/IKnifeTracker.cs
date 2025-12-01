using System.Collections.Generic;
using KnifeToss.Presentation.Gameplay.Projectiles;

namespace KnifeToss.Core.Abstractions
{
    public interface IKnifeTracker
    {
        void Register(Knife knife);
        void Unregister(Knife knife);
        IEnumerable<Knife> Snapshot();
    }
}
