using Common.MVP;
using Common.MVP.Core;
using UnityEngine;

namespace Common.MVP.Navigation
{
    public interface IUINavigationService
    {
        void Open<TPresenter, TView, TModel>(BaseMVPFactorySO<TPresenter, TView, TModel> factory, TModel model, Transform parent)
            where TPresenter : BasePresenter<TView, TModel>
            where TView : MonoBehaviour, IView<TModel>
            where TModel : IModel;

        void CloseTop();
        void CloseAll();
        void CloseByType<TPresenter>();

        void ReplaceTop<TPresenter, TView, TModel>(BaseMVPFactorySO<TPresenter, TView, TModel> factory, TModel model, Transform parent)
            where TPresenter : BasePresenter<TView, TModel>
            where TView : MonoBehaviour, IView<TModel>
            where TModel : IModel;
    }
}
