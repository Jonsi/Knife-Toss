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
    }
}
