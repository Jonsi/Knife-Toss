using Common.Events;
using UnityEngine;

namespace Game.Missions.Events
{
    /// <summary>
    /// Event channel for mission completion notifications.
    /// </summary>
    [CreateAssetMenu(fileName = "MissionCompletedEventChannel", menuName = "Knife Toss/Events/Missions/Mission Completed")]
    public class MissionCompletedEventChannel : EventChannel<MissionCompletedEventData>
    {
    }
}
