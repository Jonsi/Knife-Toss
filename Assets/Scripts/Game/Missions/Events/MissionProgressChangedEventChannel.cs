using Common.Events;
using UnityEngine;

namespace Game.Missions.Events
{
    /// <summary>
    /// Event channel for mission progress updates.
    /// </summary>
    [CreateAssetMenu(fileName = "MissionProgressChangedEventChannel", menuName = "Knife Toss/Events/Missions/Mission Progress Changed")]
    public class MissionProgressChangedEventChannel : EventChannel<MissionProgressChangedEventData>
    {
    }
}
