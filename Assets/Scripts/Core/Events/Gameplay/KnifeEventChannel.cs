using UnityEngine;

namespace KnifeToss.Core.Events.Gameplay
{
    /// <summary>
    /// Event channel for knife-related events with Vector3 payload (position).
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeEventChannel", menuName = "Knife Toss/Events/Gameplay/Knife Event Channel")]
    public class KnifeEventChannel : GenericEventChannel<Vector3>
    {
    }

    /// <summary>
    /// Event channel for knife spawned events.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSpawnedChannel", menuName = "Knife Toss/Events/Gameplay/Knife Spawned Channel")]
    public class KnifeSpawnedChannel : VoidEventChannel
    {
    }

    /// <summary>
    /// Event channel for knife thrown events.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeThrownChannel", menuName = "Knife Toss/Events/Gameplay/Knife Thrown Channel")]
    public class KnifeThrownChannel : VoidEventChannel
    {
    }

    /// <summary>
    /// Event channel for knife stuck events.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeStuckChannel", menuName = "Knife Toss/Events/Gameplay/Knife Stuck Channel")]
    public class KnifeStuckChannel : GenericEventChannel<Vector3>
    {
    }

    /// <summary>
    /// Event channel for knife bounced events.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeBouncedChannel", menuName = "Knife Toss/Events/Gameplay/Knife Bounced Channel")]
    public class KnifeBouncedChannel : GenericEventChannel<Vector3>
    {
    }

    /// <summary>
    /// Event channel for knife returned to pool events.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeReturnedToPoolChannel", menuName = "Knife Toss/Events/Gameplay/Knife Returned To Pool Channel")]
    public class KnifeReturnedToPoolChannel : VoidEventChannel
    {
    }
}

