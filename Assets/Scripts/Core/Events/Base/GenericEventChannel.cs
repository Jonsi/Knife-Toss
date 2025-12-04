using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    /// <summary>
    /// Generic base class for ScriptableObject event channels with typed payloads.
    /// Provides type-safe event broadcasting that survives domain reload.
    /// </summary>
    public abstract class GenericEventChannel<T> : ScriptableObject
    {
        private readonly List<Action<T>> _listeners = new List<Action<T>>();

        /// <summary>
        /// Raises the event with the specified value to all subscribers.
        /// </summary>
        public void Raise(T value)
        {
            // Iterate backwards to allow removal during iteration
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        /// <summary>
        /// Subscribes a listener to this event channel.
        /// </summary>
        public void Subscribe(Action<T> listener)
        {
            if (listener == null)
            {
                return;
            }

            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unsubscribes a listener from this event channel.
        /// </summary>
        public void Unsubscribe(Action<T> listener)
        {
            if (listener == null)
            {
                return;
            }

            _listeners.Remove(listener);
        }

        /// <summary>
        /// Clears all listeners. Useful for cleanup.
        /// </summary>
        public void Clear()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// Gets the current number of subscribers.
        /// </summary>
        public int ListenerCount => _listeners.Count;
    }
}

