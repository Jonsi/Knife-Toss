using Common.Events;
using Game.Flow.Level;
using UnityEngine;

namespace Game.Flow.Events
{
    [CreateAssetMenu(fileName = "LevelFailedEventChannel", menuName = "Knife Toss/Events/Flow/Level Failed")]
    public class LevelFailedEventChannel : EventChannel<LevelData>
    {
    }
}
