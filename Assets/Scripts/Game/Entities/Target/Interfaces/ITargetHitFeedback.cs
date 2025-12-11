namespace Game.Entities.Target.Interfaces
{
    /// <summary>
    /// Plays visual feedback for target hits.
    /// </summary>
    public interface ITargetHitFeedback
    {
        /// <summary>
        /// Plays the configured hit feedback animation.
        /// </summary>
        void PlayHitFeedback();
    }
}

