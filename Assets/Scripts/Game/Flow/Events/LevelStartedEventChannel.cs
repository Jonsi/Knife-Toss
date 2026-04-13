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
        public bool HasLastEvent { get; private set; }
        public LevelStartedEventData LastEvent { get; private set; }

        public new void Raise(LevelStartedEventData data)
        {
            LastEvent = data;
            HasLastEvent = data != null;
            base.Raise(data);
        }

        public void ClearLastEvent()
        {
            LastEvent = null;
            HasLastEvent = false;
        }
    }
}
