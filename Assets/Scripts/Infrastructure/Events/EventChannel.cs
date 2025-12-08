using System;
using Core.Abstractions;
using UnityEngine;

namespace Infrastructure.Events
{
    /// <summary>
    /// Generic event channel using ScriptableObject pattern for decoupled communication.
    /// </summary>
    /// <typeparam name="TData">The type of data passed with the event.</typeparam>
    public abstract class EventChannel<TData> : ScriptableObject, IEventChannel<TData>
    {
        private event Action<TData> OnEventRaised;

        /// <summary>
        /// Raises the event with the specified data.
        /// </summary>
        /// <param name="data">The data to pass with the event.</param>
        public void Raise(TData data)
        {
            OnEventRaised?.Invoke(data);
        }

        /// <summary>
        /// Subscribes to the event.
        /// </summary>
        /// <param name="handler">The event handler to subscribe.</param>
        public void Subscribe(Action<TData> handler)
        {
            OnEventRaised += handler;
        }

        /// <summary>
        /// Unsubscribes from the event.
        /// </summary>
        /// <param name="handler">The event handler to unsubscribe.</param>
        public void Unsubscribe(Action<TData> handler)
        {
            OnEventRaised -= handler;
        }
    }
}

