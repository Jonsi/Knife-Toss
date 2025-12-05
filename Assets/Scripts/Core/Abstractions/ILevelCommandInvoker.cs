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
    }
}
