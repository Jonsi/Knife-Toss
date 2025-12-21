namespace Common.MVP.Core
{
    public interface IView<TModel> where TModel : IModel
    {
        void Initialize(TModel model);
        void Show();
        void Hide();
    }
}
