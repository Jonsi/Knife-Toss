using UnityEngine;

namespace Common.MVP.Core
{
    public interface IFactory<out TPresenter, in TView, in TModel>
        where TView : MonoBehaviour
        where TModel : IModel
    {
        TPresenter Create(TView prefab, TModel model, Transform parent);
    }
}
