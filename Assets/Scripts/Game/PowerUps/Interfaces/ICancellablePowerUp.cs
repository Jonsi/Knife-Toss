namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Interface for powerups that can be cancelled after activation.
    /// </summary>
    public interface ICancellablePowerUp
    {
        /// <summary>
        /// Called when the powerup is cancelled without being used.
        /// </summary>
        void OnCancelled();
    }
}
