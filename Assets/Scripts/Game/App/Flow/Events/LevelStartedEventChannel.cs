using Common.Events;
using UnityEngine;

namespace Game.App.Flow.Events
{
    /// <summary>
    /// Event channel raised when a level starts.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelStartedEventChannel", menuName = "Knife Toss/Events/Level Started Event Channel")]
    public class LevelStartedEventChannel : EventChannel<LevelStartedEventData>
    {
    }
}
