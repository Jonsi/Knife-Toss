using KnifeToss.Core.Abstractions.Commands;

namespace KnifeToss.Core.Data.Commands
{
    /// <summary>
    /// Event payload that wraps an executable command.
    /// </summary>
    public struct CommandEventData
    {
        public ICommand Command { get; }

        public CommandEventData(ICommand command)
        {
            Command = command;
        }
    }
}

