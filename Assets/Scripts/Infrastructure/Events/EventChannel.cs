using System;
using System.Collections.Generic;
using Core.Abstractions;
using UnityEngine;

namespace Infrastructure.Events
{
    /// <summary>
    /// Very simple event channel where subscribers are stored by owner (handler.Target).
    /// No duplicate handlers per owner. Raise invokes all registered callbacks.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject, IEventChannel<T>
    {
        // Owner -> handler
        private readonly Dictionary<object, Action<T>> _subscriptions = new();

        public void Raise(T data)
        {
            foreach (var callback in _subscriptions.Values)
                callback?.Invoke(data);
        }

        public void Subscribe(Action<T> handler)
        {
            if (handler == null) return;
            var owner = handler.Target;
            _subscriptions[owner] = handler;
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (handler == null) return;
            var owner = handler.Target;
            _subscriptions.Remove(owner);
        }

        public void UnsubscribeOwner(object owner)
        {
            if (owner == null) return;
            _subscriptions.Remove(owner);
        }

        public void UnsubscribeAll() => _subscriptions.Clear();
    }
}
