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

        public Observable<Unit> RetryRequested => _RetryButton.OnClickAsObservable();
        public Observable<Unit> RestartRequested => _RestartStageButton.OnClickAsObservable();

        public void Initialize(StageFailPopupModel model) { }
    }
}
