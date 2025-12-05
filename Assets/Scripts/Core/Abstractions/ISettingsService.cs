namespace Core.Abstractions
{
    /// <summary>
    /// Defines the ISettingsService interface.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Performs the ToggleMute operation.
        /// </summary>
        void ToggleMute();
        /// <summary>
        /// Performs the IsMuted operation.
        /// </summary>
        bool IsMuted();
    }
}
