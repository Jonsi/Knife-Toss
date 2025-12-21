using Common.MVP.Core;
using Game.UI.Common;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    public class StageFailPopupView : BasePopup, IView<StageFailPopupModel>
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;

        private readonly Subject<Unit> _RetryRequested = new Subject<Unit>();
        private readonly Subject<Unit> _RestartRequested = new Subject<Unit>();
        private StageFailPopupModel _Model;

        public Observable<Unit> RetryRequested => _RetryRequested;
        public Observable<Unit> RestartRequested => _RestartRequested;

        public void Initialize(StageFailPopupModel model)
        {
            _Model = model;
        }

        private void Awake()
        {
            if (_RetryButton != null)
            {
                _RetryButton.OnClickAsObservable()
                    .Subscribe(_ => _RetryRequested.OnNext(Unit.Default));
            }

            if (_RestartStageButton != null)
            {
                _RestartStageButton.OnClickAsObservable()
                    .Subscribe(_ => _RestartRequested.OnNext(Unit.Default));
            }
        }

        private void OnDestroy()
        {
            _RetryRequested.OnCompleted();
            _RetryRequested.Dispose();
            _RestartRequested.OnCompleted();
            _RestartRequested.Dispose();
        }
    }
}
