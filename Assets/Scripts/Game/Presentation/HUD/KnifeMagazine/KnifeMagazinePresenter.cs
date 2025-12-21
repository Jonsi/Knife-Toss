using Common.Events;
using Common.MVP.Core;
using Game.App.Flow.Events;
using R3;
using VContainer.Unity;

namespace Game.Presentation.HUD.KnifeMagazine
{
    /// <summary>
    /// Presenter that keeps the knife magazine HUD in sync with gameplay events.
    /// </summary>
    public class KnifeMagazinePresenter : BasePresenter<KnifeMagazineView, KnifeMagazineModel>, IStartable
    {
        private readonly IEventChannel<LevelStartedEventData> _LevelStartedChannel;
        private readonly IEventChannel _KnifeThrownChannel;

        public KnifeMagazinePresenter(
            KnifeMagazineView view,
            IEventChannel<LevelStartedEventData> levelStartedChannel,
            IEventChannel knifeThrownChannel)
            : base(view, new KnifeMagazineModel())
        {
            _LevelStartedChannel = levelStartedChannel;
            _KnifeThrownChannel = knifeThrownChannel;
        }

        public void Start()
        {
            View.Initialize(Model);
            Model.TotalKnives
                .Subscribe(_ => ApplyView())
                .AddTo(disposables);
            Model.KnivesRemaining
                .Subscribe(_ => ApplyView())
                .AddTo(disposables);

            _LevelStartedChannel
                ?.AsObservable()
                .Subscribe(OnLevelStarted)
                .AddTo(disposables);

            _KnifeThrownChannel
                ?.AsObservable()
                .Subscribe(_ => OnKnifeThrown())
                .AddTo(disposables);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            if (data == null)
            {
                return;
            }

            Model.TotalKnives.Value = data.TotalKnives;
            Model.KnivesRemaining.Value = data.TotalKnives;
        }

        private void OnKnifeThrown()
        {
            if (Model.KnivesRemaining.Value <= 0)
            {
                return;
            }

            Model.KnivesRemaining.Value -= 1;
        }

        private void ApplyView()
        {
            View.SetKnifeMagazine(Model.TotalKnives.Value, Model.KnivesRemaining.Value);
        }
    }
}
