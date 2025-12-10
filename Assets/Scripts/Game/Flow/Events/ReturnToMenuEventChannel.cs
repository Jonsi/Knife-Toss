using Common.Events;
using Game.Flow.Stage;
using UnityEngine;

namespace Game.Flow.Events
{
    [CreateAssetMenu(fileName = "ReturnToMenuEventChannel", menuName = "Knife Toss/Events/Flow/Return To Menu")]
    public class ReturnToMenuEventChannel : EventChannel<StageData>
    {
    }
}
