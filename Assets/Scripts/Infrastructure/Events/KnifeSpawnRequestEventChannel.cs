using KnifeToss.Core.Data;
using UnityEngine;

namespace Infrastructure.Events
{
    /// <summary>
    /// Event channel for knife spawn requests.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSpawnRequestEventChannel", menuName = "Knife Toss/Events/Knife Spawn Request Event Channel")]
    public class KnifeSpawnRequestEventChannel : EventChannel<KnifeSpawnRequestData>
    {
    }
}

