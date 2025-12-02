namespace KnifeToss.Core.Abstractions
{
    public interface ISettingsService
    {
        void ToggleMute();
        bool IsMuted();
    }
}
