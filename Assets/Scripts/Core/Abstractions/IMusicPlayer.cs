using Cysharp.Threading.Tasks;

namespace KnifeToss.Core.Abstractions
{
    public interface IMusicPlayer
    {
        void PlayMusic();
        void StopMusic();
        void SetMuted(bool isMuted);
    }
}
