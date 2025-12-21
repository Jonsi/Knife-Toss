using Common.MVP.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.UI.Common
{
    /// <summary>
    /// Base class for UI popups providing common show/hide functionality.
    /// </summary>
    public abstract class BasePopupView<TModel> : MonoBehaviour, IView<TModel>
        where TModel : IModel
    {
        public virtual void Initialize(TModel model){}

        public virtual UniTask Show()
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public virtual UniTask Hide()
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}
