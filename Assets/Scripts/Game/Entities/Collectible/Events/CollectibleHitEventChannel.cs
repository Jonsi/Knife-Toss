using Common.Events;
using Game.Entities.Collectible.Data;
using UnityEngine;

namespace Game.Entities.Collectible.Events
{
    /// <summary>
    /// Event channel for when a collectible is hit.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleHitEventChannel", menuName = "Knife Toss/Events/Collectible Hit Event Channel")]
    public class CollectibleHitEventChannel : EventChannel<CollectibleHitEventData>
    {
    }
}

