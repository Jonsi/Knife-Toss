using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using VContainer.Unity;

namespace KnifeToss.Application.Services
{
    /// <summary>
    /// Represents the ScoreService class.
    /// </summary>
    public class ScoreService : IScoreService, IStartable, System.IDisposable
    {
        private const int ScorePerHit = 100;

        private readonly LevelSession _levelSession;
        private readonly KnifeImpactEventChannel _knifeImpactEventChannel;

        /// <summary>
        /// Performs the ScoreService operation.
        /// </summary>
        public ScoreService(LevelSession levelSession, KnifeImpactEventChannel knifeImpactEventChannel)
        {
            _levelSession = levelSession;
            _knifeImpactEventChannel = knifeImpactEventChannel;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            _knifeImpactEventChannel.KnifeStuckAt += HandleKnifeStuck;
        }

        /// <summary>
        /// Performs the Dispose operation.
        /// </summary>
        public void Dispose()
        {
            _knifeImpactEventChannel.KnifeStuckAt -= HandleKnifeStuck;
        }

        /// <summary>
        /// Performs the HandleKnifeStuck operation.
        /// </summary>
        private void HandleKnifeStuck(UnityEngine.Vector3 impactPoint)
        {
            _levelSession.AddScore(ScorePerHit);
        }
    }
}
