using UnityEngine;

namespace Common.MVP
{
    public interface IFactory<out TPresenter, in TView, in TModel>
        where TView : MonoBehaviour
        where TModel : IModel
    {
        TPresenter Create(TView prefab, TModel model, Transform parent);
    }
}
