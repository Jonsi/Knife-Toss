namespace KnifeToss.Core.Abstractions.Commands
{
    /// <summary>
    /// Generic interface for any executable command in the system.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command logic.
        /// </summary>
        void Execute();
    }
}

