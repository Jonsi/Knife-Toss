using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    /// <summary>
    /// Event channel for events without payloads (void events).
    /// </summary>
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Knife Toss/Events/Void Event Channel")]
    public class VoidEventChannel : ScriptableObject
    {
        private readonly List<Action> _listeners = new List<Action>();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke();
            }
        }

        public void Subscribe(Action listener)
        {
            if (listener == null) return;
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void Unsubscribe(Action listener)
        {
            if (listener == null) return;
            _listeners.Remove(listener);
        }

        public void Clear()
        {
            _listeners.Clear();
        }

        public int ListenerCount => _listeners.Count;
    }
}

