using Common.Events;
using UnityEngine;

namespace Game.Flow.Events
{
    /// <summary>
    /// Event channel raised when a level starts.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelStartedEventChannel", menuName = "Knife Toss/Events/Flow/Level Started")]
    public class LevelStartedEventChannel : EventChannel<LevelStartedEventData>
    {
    }
}
