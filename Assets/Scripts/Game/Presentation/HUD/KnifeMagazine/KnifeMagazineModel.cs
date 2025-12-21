using Common.MVP.Core;
using R3;

namespace Game.Presentation.HUD.KnifeMagazine
{
    /// <summary>
    /// Reactive state for the knife magazine HUD.
    /// </summary>
    public class KnifeMagazineModel : IModel
    {
        public ReactiveProperty<int> TotalKnives { get; } = new(0);
        public ReactiveProperty<int> KnivesRemaining { get; } = new(0);
    }
}
