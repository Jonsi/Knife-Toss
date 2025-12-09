using KnifeToss.Core.Data.Level;
using UnityEngine;

namespace Infrastructure.Events
{
    /// <summary>
    /// Event channel for level state transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelStateChangedEventChannel", menuName = "Knife Toss/Events/Level State Changed Event Channel")]
    public class LevelStateChangedEventChannel : EventChannel<LevelStateChangedEventData>
    {
    }
}

