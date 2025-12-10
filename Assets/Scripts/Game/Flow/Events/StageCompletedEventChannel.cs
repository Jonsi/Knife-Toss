using Common.Events;
using Game.Flow.Stage;
using UnityEngine;

namespace Game.Flow.Events
{
    [CreateAssetMenu(fileName = "StageCompletedEventChannel", menuName = "Knife Toss/Events/Flow/Stage Completed")]
    public class StageCompletedEventChannel : EventChannel<StageData>
    {
    }
}
