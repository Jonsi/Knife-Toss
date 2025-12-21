using Common.MVP.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Presentation.Common
{
    /// <summary>
    /// Base view implementation for MVP-driven UI.
    /// </summary>
    public abstract class BaseView<TModel> : MonoBehaviour, IView<TModel>
        where TModel : IModel
    {
        public virtual void Initialize(TModel model)
        {
        }

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
