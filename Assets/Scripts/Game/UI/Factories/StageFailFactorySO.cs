using Common.MVP;
using Game.UI.Popups;
using Game.UI.Presenters;
using UnityEngine;

namespace Game.UI.Factories
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
