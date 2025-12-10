using System;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Data.Commands;
using VContainer.Unity;

namespace App.Services.Commands
{
    /// <summary>
    /// Listens to command events and executes them.
    /// </summary>
    public class CommandDispatcher : IStartable, IDisposable
    {
        private readonly IEventChannel<CommandEventData> _Channel;

        public CommandDispatcher(IEventChannel<CommandEventData> channel)
        {
            _Channel = channel;
        }

        public void Start()
        {
            _Channel.Subscribe(OnCommandReceived);
        }

        public void Dispose()
        {
            _Channel.Unsubscribe(OnCommandReceived);
        }

        private void OnCommandReceived(CommandEventData data)
        {
            data.Command?.Execute();
        }
    }
}

