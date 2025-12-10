using Common.Events;
using Game.Collision.Data;
using UnityEngine;

namespace Game.Collision.Events
{
    /// <summary>
    /// ScriptableObject event channel for collision events.
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionEventChannel", menuName = "Knife Toss/Events/Collision Event Channel")]
    public class CollisionEventChannel : EventChannel<CollisionData>
    {
    }
}

