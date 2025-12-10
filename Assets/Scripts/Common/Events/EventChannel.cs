using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Events
{
    /// <summary>
    /// Very simple event channel where subscribers are stored by owner (handler.Target).
    /// No duplicate handlers per owner. Raise invokes all registered callbacks.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject, IEventChannel<T>
    {
        private readonly Dictionary<object, Action<T>> _Subscriptions = new();

        /// <summary>
        /// Invokes the event for all subscribers with the provided data.
        /// </summary>
        /// <param name="data">Payload for the event.</param>
        public void Raise(T data)
        {
            foreach (var callback in _Subscriptions.Values)
            {
                callback?.Invoke(data);
            }
        }

        /// <summary>
        /// Adds or replaces the handler for the given owner.
        /// </summary>
        /// <param name="handler">Event handler to register.</param>
        public void Subscribe(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            var owner = handler.Target;
            _Subscriptions[owner] = handler;
        }

        /// <summary>
        /// Removes the handler for the given delegate owner.
        /// </summary>
        /// <param name="handler">Event handler to remove.</param>
        public void Unsubscribe(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            var owner = handler.Target;
            _Subscriptions.Remove(owner);
        }

        /// <summary>
        /// Removes all handlers for the specified owner object.
        /// </summary>
        /// <param name="owner">Owner instance used as subscription key.</param>
        public void UnsubscribeOwner(object owner)
        {
            if (owner == null)
            {
                return;
            }

            _Subscriptions.Remove(owner);
        }

        /// <summary>
        /// Clears all subscriptions.
        /// </summary>
        public void UnsubscribeAll()
        {
            _Subscriptions.Clear();
        }
    }
    
    /// <summary>
    /// Event channel for void events (no data passed).
    /// </summary>
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Knife Toss/Events/Void Event Channel")]
    public class VoidEventChannel : ScriptableObject, IEventChannel
    {
        private readonly Dictionary<object, Action> _subscriptions = new();

        public void Raise()
        {
            foreach (var callback in _subscriptions.Values)
            {
                callback?.Invoke();
            }
        }

        public void Subscribe(Action handler)
        {
            if (handler == null)
                return;

            var owner = handler.Target;
            _subscriptions[owner] = handler;
        }

        public void Unsubscribe(Action handler)
        {
            if (handler == null)
                return;

            var owner = handler.Target;
            _subscriptions.Remove(owner);
        }

        public void UnsubscribeAll()
        {
            _subscriptions.Clear();
        }
    }
}
