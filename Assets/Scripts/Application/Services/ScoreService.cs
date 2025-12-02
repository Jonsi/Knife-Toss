using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using VContainer.Unity;

namespace KnifeToss.Application.Services
{
    public class ScoreService : IScoreService, IStartable, System.IDisposable
    {
        private const int ScorePerHit = 100;

        private readonly LevelSession _levelSession;
        private readonly KnifeImpactEventChannel _knifeImpactEventChannel;

        public ScoreService(LevelSession levelSession, KnifeImpactEventChannel knifeImpactEventChannel)
        {
            _levelSession = levelSession;
            _knifeImpactEventChannel = knifeImpactEventChannel;
        }

        public void Start()
        {
            _knifeImpactEventChannel.KnifeStuckAt += HandleKnifeStuck;
        }

        public void Dispose()
        {
            _knifeImpactEventChannel.KnifeStuckAt -= HandleKnifeStuck;
        }

        private void HandleKnifeStuck(UnityEngine.Vector3 impactPoint)
        {
            _levelSession.AddScore(ScorePerHit);
        }
    }
}
