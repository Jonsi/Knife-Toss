using System;
using System.Collections.Generic;
using Common.MVP.Core;
using UnityEngine;
using Common.MVP;

namespace Common.MVP.Popup
{
    public class PopupNavigationService : IPopupNavigationService
    {
        private readonly Stack<IDisposable> _activePresenters = new();

        public void Open<TPresenter, TView, TModel>(BaseMVPFactorySO<TPresenter, TView, TModel> factory, TModel model, Transform parent)
            where TPresenter : BasePresenter<TView, TModel>
            where TView : MonoBehaviour, IView<TModel>
            where TModel : IModel
        {
            OpenPopup(factory, model, parent);
        }

        public void OpenPopup<TPresenter, TView, TModel>(BaseMVPFactorySO<TPresenter, TView, TModel> factory, TModel model, Transform parent)
            where TPresenter : BasePresenter<TView, TModel>
            where TView : MonoBehaviour, IView<TModel>
            where TModel : IModel
        {
            TPresenter presenter = factory.Create(null, model, parent);
            _activePresenters.Push(presenter);
        }

        public void CloseTop()
        {
            if (_activePresenters.Count == 0)
                return;

            var top = _activePresenters.Pop();
            top.Dispose();
        }

        public void CloseAll()
        {
            while (_activePresenters.Count > 0)
            {
                var presenter = _activePresenters.Pop();
                presenter.Dispose();
            }
        }

        public void CloseByType<TPresenter>()
        {
            if (_activePresenters.Count == 0)
                return;

            var buffer = new Stack<IDisposable>();
            var removed = false;

            while (_activePresenters.Count > 0)
            {
                var presenter = _activePresenters.Pop();
                if (!removed && presenter is TPresenter)
                {
                    presenter.Dispose();
                    removed = true;
                    break;
                }

                buffer.Push(presenter);
            }

            while (buffer.Count > 0)
            {
                _activePresenters.Push(buffer.Pop());
            }
        }

        public void ReplaceTop<TPresenter, TView, TModel>(BaseMVPFactorySO<TPresenter, TView, TModel> factory, TModel model, Transform parent)
            where TPresenter : BasePresenter<TView, TModel>
            where TView : MonoBehaviour, IView<TModel>
            where TModel : IModel
        {
            CloseTop();
            OpenPopup(factory, model, parent);
        }
    }
}
