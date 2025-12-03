using Cysharp.Threading.Tasks;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Defines the IMusicPlayer interface.
    /// </summary>
    public interface IMusicPlayer
    {
        /// <summary>
        /// Performs the PlayMusic operation.
        /// </summary>
        void PlayMusic();
        /// <summary>
        /// Performs the StopMusic operation.
        /// </summary>
        void StopMusic();
        /// <summary>
        /// Performs the SetMuted operation.
        /// </summary>
        void SetMuted(bool isMuted);
    }
}
