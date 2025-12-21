using System;

namespace Common.MVP.Core
{
    public interface IPresenter<out TView, out TModel> : IDisposable
        where TView : IView<TModel>
        where TModel : IModel
    {
        TView View { get; }
        TModel Model { get; }
    }
}
