using Common.MVP.Core;
using R3;

namespace Game.Presentation.HUD.Collectibles
{
    /// <summary>
    /// Reactive state for collectible count HUD.
    /// </summary>
    public class CollectibleCountModel : IModel
    {
        public ReactiveProperty<int> TotalCollectibles { get; } = new(0);
    }
}
