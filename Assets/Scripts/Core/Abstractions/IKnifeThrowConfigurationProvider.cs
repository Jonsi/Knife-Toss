using Core.Data;

namespace Core.Abstractions
{
    /// <summary>
    /// Interface for providers of knife throw configuration data.
    /// </summary>
    public interface IKnifeThrowConfigurationProvider
    {
        /// <summary>
        /// Gets the knife throw configuration.
        /// </summary>
        /// <returns>A KnifeThrowConfiguration object containing the configuration values.</returns>
        KnifeThrowConfiguration GetConfiguration();
    }
}

