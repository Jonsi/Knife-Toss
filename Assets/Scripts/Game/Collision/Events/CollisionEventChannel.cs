using KnifeToss.Common.Events;
using KnifeToss.Game.Collision.Data;
using UnityEngine;

namespace KnifeToss.Game.Collision.Events
{
    /// <summary>
    /// ScriptableObject event channel for collision events.
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionEventChannel", menuName = "Knife Toss/Events/Collision Event Channel")]
    public class CollisionEventChannel : EventChannel<CollisionData>
    {
    }
}

