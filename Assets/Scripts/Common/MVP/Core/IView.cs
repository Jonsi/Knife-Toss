using Cysharp.Threading.Tasks;

namespace Common.MVP.Core
{
    public interface IView<TModel> where TModel : IModel
    {
        void Initialize(TModel model);
        UniTask Show();
        UniTask Hide();
    }
}
