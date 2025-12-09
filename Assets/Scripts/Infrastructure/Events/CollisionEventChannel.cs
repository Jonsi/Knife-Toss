using Core.Data;
using UnityEngine;

namespace Infrastructure.Infrastructure.Events
{
    /// <summary>
    /// ScriptableObject event channel for collision events.
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionEventChannel", menuName = "Knife Toss/Events/Collision Event Channel")]
    public class CollisionEventChannel : EventChannel<CollisionData> { }
}

