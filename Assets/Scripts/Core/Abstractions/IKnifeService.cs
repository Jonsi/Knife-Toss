using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.Gameplay.Projectiles;

namespace KnifeToss.Core.Abstractions
{
    public interface IKnifeService
    {
        void Configure(int totalKnives);
        Knife GetNextKnife();
        void Return(Knife knife);
        void SlideQueueForward();
        void Reset();
    }
}
