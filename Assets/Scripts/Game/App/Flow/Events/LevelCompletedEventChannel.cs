using Common.Events;
using Game.Flow.Level;
using UnityEngine;

namespace Game.App.Flow.Events
{
    [CreateAssetMenu(fileName = "LevelCompletedEventChannel", menuName = "Knife Toss/Events/Flow/Level Completed")]
    public class LevelCompletedEventChannel : EventChannel<LevelData>
    {
    }
}
