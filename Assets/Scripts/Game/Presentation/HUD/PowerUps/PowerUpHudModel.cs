using Common.MVP.Core;
using Game.PowerUps.Configs;
using R3;

namespace Game.Presentation.HUD.PowerUps
{
    /// <summary>
    /// Reactive state for the power-up HUD.
    /// </summary>
    public class PowerUpHudModel : IModel
    {
        public ReactiveProperty<int> SuperKnifeCount { get; } = new(0);
        public ReactiveProperty<int> MultipleKnivesCount { get; } = new(0);
        public ReactiveProperty<int> CollectiblesBladeCount { get; } = new(0);
        public ReactiveProperty<PowerUpConfigBase> SelectedConfig { get; } = new(null);
    }
}
