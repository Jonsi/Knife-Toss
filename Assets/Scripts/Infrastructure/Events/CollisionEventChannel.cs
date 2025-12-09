using KnifeToss.Core.Data;
using UnityEngine;

namespace Infrastructure.Events
{
    /// <summary>
    /// ScriptableObject event channel for collision events.
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionEventChannel", menuName = "Knife Toss/Events/Collision Event Channel")]
    public class CollisionEventChannel : EventChannel<CollisionData> { }
}

