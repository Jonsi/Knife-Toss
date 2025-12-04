// GameEventBus.cs
using System;
using System.Collections.Generic;

namespace KnifeToss.Application
{
    /// <inheritdoc />
    public sealed class GameEventBus : IGameEventBus
    {
        private readonly Dictionary<Type, object> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var listObj))
            {
                listObj = new List<Action<T>>();
                _handlers[type] = listObj;
            }
            ((List<Action<T>>)listObj).Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var listObj))
            {
                ((List<Action<T>>)listObj).Remove(handler);
            }
        }

        public void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var listObj))
            {
                foreach (var handler in ((List<Action<T>>)listObj).ToArray())
                {
                    handler.Invoke(evt);
                }
            }
        }
    }
}

