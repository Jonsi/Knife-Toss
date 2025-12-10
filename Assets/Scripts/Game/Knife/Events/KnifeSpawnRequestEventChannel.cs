using KnifeToss.Common.Events;
using KnifeToss.Game.Knife.Data;
using UnityEngine;

namespace KnifeToss.Game.Knife.Events
{
    /// <summary>
    /// Event channel for knife spawn requests.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSpawnRequestEventChannel", menuName = "Knife Toss/Events/Knife Spawn Request Event Channel")]
    public class KnifeSpawnRequestEventChannel : EventChannel<KnifeSpawnRequestData>
    {
    }
}

