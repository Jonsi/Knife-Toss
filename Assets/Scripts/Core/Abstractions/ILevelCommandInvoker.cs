namespace Core.Abstractions
{
    /// <summary>
    /// Defines the ILevelCommandInvoker interface.
    /// </summary>
    public interface ILevelCommandInvoker
    {
        /// <summary>
        /// Performs the Retry operation.
        /// </summary>
        void Retry();
        /// <summary>
        /// Performs the Next operation.
        /// </summary>
        void Next();

        /// <summary>
        /// Requests loading the specified level index.
        /// </summary>
        /// <param name="levelIndex">The level (1-based) to load.</param>
        void LoadLevel(int levelIndex);
    }
}
