namespace Common.MVP.Core
{
    public abstract class BasePresenter<TView, TModel> : IPresenter<TView, TModel>
        where TView : class, IView<TModel>
        where TModel : IModel
    {
        public TView View { get; }
        public TModel Model { get; }

        protected BasePresenter(TView view, TModel model)
        {
            View = view;
            Model = model;
        }

        public abstract void Dispose();
    }
}
