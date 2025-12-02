
using KnifeToss.Core.Data;

namespace KnifeToss.Core.Abstractions
{
    public interface ISettingsRepository
    {
        void Save(AudioSettingsData settings);
        AudioSettingsData Load();
    }
}
