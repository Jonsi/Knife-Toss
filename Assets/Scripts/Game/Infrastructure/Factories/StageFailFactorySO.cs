using Common.MVP;
using Game.Presentation.Popups.StageFail;
using UnityEngine;

namespace Game.Infrastructure.Factories
{
    [CreateAssetMenu(fileName = "StageFailFactory", menuName = "MVP/Factories/UI/StageFail")]
    public class StageFailFactorySO : BaseMVPFactorySO<StageFailPopupPresenter, StageFailPopupView, StageFailPopupModel>
    {
        protected override StageFailPopupPresenter CreatePresenter(
            StageFailPopupView view,
            StageFailPopupModel model,
            System.Action<StageFailPopupView> release)
        {
            return new StageFailPopupPresenter(view, model, release);
        }
    }
}
