using KnifeToss.Common.Events;
using KnifeToss.Game.Level.Data;
using UnityEngine;

namespace KnifeToss.Game.Level.Events
{
    /// <summary>
    /// Event channel for level state transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelStateChangedEventChannel", menuName = "Knife Toss/Events/Level State Changed Event Channel")]
    public class LevelStateChangedEventChannel : EventChannel<LevelStateChangedEventData>
    {
    }
}

