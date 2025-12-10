using Common.Collision.Data;
using Common.Events;
using UnityEngine;

namespace Common.Collision.Events
{
    /// <summary>
    /// ScriptableObject event channel for collision events.
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionEventChannel", menuName = "Knife Toss/Events/Collision Event Channel")]
    public class CollisionEventChannel : EventChannel<CollisionData>
    {
    }
}

