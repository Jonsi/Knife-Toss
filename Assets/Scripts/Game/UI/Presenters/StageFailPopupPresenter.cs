using System;
using Common.MVP.Core;
using Game.UI.Popups;
using R3;
using Cysharp.Threading.Tasks;

namespace Game.UI.Presenters
{
    public class StageFailPopupPresenter : BasePresenter<StageFailPopupView, StageFailPopupModel>
    {
        private readonly Action<StageFailPopupView> _ReleaseView;

        public StageFailPopupPresenter(
            StageFailPopupView view,
            StageFailPopupModel model,
            Action<StageFailPopupView> releaseView) : base(view, model)
        {
            _ReleaseView = releaseView;

            View.Initialize(Model);
            View.Show().Forget();
            View.RetryRequested
                .Subscribe(_ => HandleRetryRequested())
                .AddTo(Disposable);

            View.RestartRequested
                .Subscribe(_ => HandleRestartRequested())
                .AddTo(Disposable);
        }

        public override void Dispose()
        {
            base.Dispose();
            _ReleaseView?.Invoke(View);
        }

        private void HandleRetryRequested()
        {
            Model.RetryAction?.Invoke();
            Model.CloseAction?.Invoke();
        }

        private void HandleRestartRequested()
        {
            Model.RestartAction?.Invoke();
            Model.CloseAction?.Invoke();
        }
    }
}
