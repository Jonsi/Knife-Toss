using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.Gameplay.Projectiles;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Defines the IKnifeService interface.
    /// </summary>
    public interface IKnifeService
    {
        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        void Configure(int totalKnives);
        Knife GetNextKnife();
        /// <summary>
        /// Performs the Return operation.
        /// </summary>
        void Return(Knife knife);
        /// <summary>
        /// Performs the SlideQueueForward operation.
        /// </summary>
        void SlideQueueForward();
        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        void Reset();
    }
}

