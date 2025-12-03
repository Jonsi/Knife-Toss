
using KnifeToss.Core.Data;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Defines the ISettingsRepository interface.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        /// Performs the Save operation.
        /// </summary>
        void Save(AudioSettingsData settings);
        AudioSettingsData Load();
    }
}
