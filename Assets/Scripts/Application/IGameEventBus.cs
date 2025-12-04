// IGameEventBus.cs
using System;

namespace KnifeToss.Application
{
    /// <summary>
    /// Simple pub/sub event bus used by Application & Presentation layers.
    /// Thread-safe for Unity main thread usage only.
    /// </summary>
    public interface IGameEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T evt);
    }
}

