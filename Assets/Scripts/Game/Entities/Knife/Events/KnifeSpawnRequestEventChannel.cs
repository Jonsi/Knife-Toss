using Common.Events;
using Game.Entities.Knife.Data;
using UnityEngine;

namespace Game.Entities.Knife.Events
{
    /// <summary>
    /// Event channel for knife spawn requests.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSpawnRequestEventChannel", menuName = "Knife Toss/Events/Knife Spawn Request Event Channel")]
    public class KnifeSpawnRequestEventChannel : EventChannel<KnifeSpawnRequestData>
    {
    }
}

