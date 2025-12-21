using Common.MVP.Core;

namespace Game.Presentation.Common
{
    /// <summary>
    /// Base class for UI popups providing common show/hide functionality.
    /// </summary>
    public abstract class BasePopupView<TModel> : BaseView<TModel>
        where TModel : IModel
    {
    }
}
