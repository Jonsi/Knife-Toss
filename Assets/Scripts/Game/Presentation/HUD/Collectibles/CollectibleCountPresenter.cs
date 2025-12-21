using Common.Events;
using Common.MVP.Core;
using Game.App.Flow.Events;
using Game.Entities.Collectible.Data;
using Game.Flow.Game;
using R3;
using VContainer.Unity;

namespace Game.Presentation.HUD.Collectibles
{
    /// <summary>
    /// Presenter that updates the collectible counter from game events.
    /// </summary>
    public class CollectibleCountPresenter : BasePresenter<CollectibleCountView, CollectibleCountModel>, IStartable
    {
        private readonly GameData _GameData;
        private readonly IEventChannel<LevelStartedEventData> _LevelStartedChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;

        public CollectibleCountPresenter(
            CollectibleCountView view,
            GameData gameData,
            IEventChannel<LevelStartedEventData> levelStartedChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel)
            : base(view, new CollectibleCountModel())
        {
            _GameData = gameData;
            _LevelStartedChannel = levelStartedChannel;
            _CollectibleHitChannel = collectibleHitChannel;
        }

        public void Start()
        {
            View.Initialize(Model);
            Model.TotalCollectibles
                .Subscribe(total => View.SetCollectibleCount(total))
                .AddTo(disposables);

            _LevelStartedChannel
                ?.AsObservable()
                .Subscribe(_ => SyncFromGameData())
                .AddTo(disposables);

            _CollectibleHitChannel
                ?.AsObservable()
                .Subscribe(OnCollectibleHit)
                .AddTo(disposables);

            SyncFromGameData();
        }

        private void SyncFromGameData()
        {
            if (_GameData == null)
            {
                return;
            }

            Model.TotalCollectibles.Value = _GameData.TotalCollected;
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            Model.TotalCollectibles.Value += data.Value;
        }
    }
}
