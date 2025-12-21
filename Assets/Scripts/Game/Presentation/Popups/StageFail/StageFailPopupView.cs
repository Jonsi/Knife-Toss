using Game.Presentation.Common;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Popups.StageFail
{
    public class StageFailPopupView : BasePopupView<StageFailPopupModel>
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;

        public Observable<Unit> RetryRequested => _RetryButton.OnClickAsObservable();
        public Observable<Unit> RestartRequested => _RestartStageButton.OnClickAsObservable();
    }
}
