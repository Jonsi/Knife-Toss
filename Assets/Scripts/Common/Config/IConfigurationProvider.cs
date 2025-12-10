namespace Common.Config
{
    /// <summary>
    /// Generic interface for providers of configuration data.
    /// </summary>
    /// <typeparam name="T">The type of configuration data provided.</typeparam>
    public interface IConfigurationProvider<out T>
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>A configuration object of type T.</returns>
        T GetConfiguration();
    }
}

