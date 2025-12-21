using Common.MVP.Core;
using R3;

namespace Game.Presentation.HUD.StageProgress
{
    /// <summary>
    /// Reactive state for stage progress HUD.
    /// </summary>
    public class StageProgressModel : IModel
    {
        public ReactiveProperty<int> TotalLevelsInStage { get; } = new(0);
        public ReactiveProperty<int> CurrentLevelIndex { get; } = new(0);
        public ReactiveProperty<int> StageIndex { get; } = new(0);
    }
}
