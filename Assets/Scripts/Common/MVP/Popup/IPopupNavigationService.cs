using Common.MVP.Core;
using Common.MVP.Navigation;
using UnityEngine;

namespace Common.MVP.Popup
{
    public interface IPopupNavigationService : IUINavigationService
    {
        void OpenPopup<TPresenter, TView, TModel>(BaseMVPFactorySO<TPresenter, TView, TModel> factory, TModel model, Transform parent)
            where TPresenter : BasePresenter<TView, TModel>
            where TView : MonoBehaviour, IView<TModel>
            where TModel : IModel;
    }
}
