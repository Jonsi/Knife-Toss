using Common.Events;
using Common.MVP.Core;
using Game.App.Flow.Events;
using Game.Flow.Stage;
using R3;
using VContainer.Unity;

namespace Game.Presentation.HUD.StageProgress
{
    /// <summary>
    /// Presenter that updates stage progress UI based on flow events.
    /// </summary>
    public class StageProgressPresenter : BasePresenter<StageProgressView, StageProgressModel>, IStartable
    {
        private readonly IEventChannel<LevelStartedEventData> _LevelStartedChannel;
        private readonly StageCompletedEventChannel _StageCompletedChannel;

        public StageProgressPresenter(
            StageProgressView view,
            IEventChannel<LevelStartedEventData> levelStartedChannel,
            StageCompletedEventChannel stageCompletedChannel)
            : base(view, new StageProgressModel())
        {
            _LevelStartedChannel = levelStartedChannel;
            _StageCompletedChannel = stageCompletedChannel;
        }

        public void Start()
        {
            View.Initialize(Model);
            Model.TotalLevelsInStage
                .Subscribe(_ => ApplyView())
                .AddTo(disposables);
            Model.CurrentLevelIndex
                .Subscribe(_ => ApplyView())
                .AddTo(disposables);
            Model.StageIndex
                .Subscribe(_ => ApplyView())
                .AddTo(disposables);

            _LevelStartedChannel
                ?.AsObservable()
                .Subscribe(OnLevelStarted)
                .AddTo(disposables);

            _StageCompletedChannel
                ?.AsObservable()
                .Subscribe(OnStageCompleted)
                .AddTo(disposables);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            if (data == null)
            {
                return;
            }

            Model.TotalLevelsInStage.Value = data.TotalLevelsInStage;
            Model.CurrentLevelIndex.Value = data.LevelIndex;
            Model.StageIndex.Value = data.StageIndex;
        }

        private void OnStageCompleted(StageData data)
        {
            if (data == null)
            {
                return;
            }

            if (Model.StageIndex.Value >= 0 && data.StageIndex != Model.StageIndex.Value)
            {
                return;
            }

            Model.CurrentLevelIndex.Value = Model.TotalLevelsInStage.Value;
        }

        private void ApplyView()
        {
            View.SetStageProgress(
                Model.TotalLevelsInStage.Value,
                Model.CurrentLevelIndex.Value,
                Model.StageIndex.Value);
        }
    }
}
