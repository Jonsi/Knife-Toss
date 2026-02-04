using Game.Entities.Knife.Views;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Interface for the view component responsible for knife positioning and launching.
    /// Extends IProjectileThrower to include knife-specific properties.
    /// </summary>
    public interface IKnifeThrowerView : IProjectileThrower
    {
        /// <summary>
        /// Gets the currently loaded knife instance.
        /// </summary>
        KnifeView LoadedKnife { get; }
    }
}