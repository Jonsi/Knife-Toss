using Common.MVP.Core;
using R3;

namespace Game.Presentation.HUD.MuteButton
{
    /// <summary>
    /// Reactive state for the mute button.
    /// </summary>
    public class MuteButtonModel : IModel
    {
        public ReactiveProperty<bool> IsMuted { get; } = new(false);
    }
}
