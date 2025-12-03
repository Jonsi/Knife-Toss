using System;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    /// <summary>
    /// Represents the KnifeImpactEventChannel class.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Events/Knife Impact Channel")]
    public class KnifeImpactEventChannel : ScriptableObject
    {
        public event Action KnifeStuck;
        public event Action<Vector3> KnifeStuckAt;
        public event Action KnifeFailed;
        public event Action<Vector3> KnifeFailedAt;

        /// <summary>
        /// Performs the RaiseKnifeStuck operation.
        /// </summary>
        public void RaiseKnifeStuck()
        {
            KnifeStuck?.Invoke();
        }

        /// <summary>
        /// Performs the RaiseKnifeStuck operation.
        /// </summary>
        public void RaiseKnifeStuck(Vector3 position)
        {
            KnifeStuckAt?.Invoke(position);
            KnifeStuck?.Invoke();
        }

        /// <summary>
        /// Performs the RaiseKnifeFailed operation.
        /// </summary>
        public void RaiseKnifeFailed()
        {
            KnifeFailed?.Invoke();
        }

        /// <summary>
        /// Performs the RaiseKnifeFailed operation.
        /// </summary>
        public void RaiseKnifeFailed(Vector3 position)
        {
            KnifeFailedAt?.Invoke(position);
            KnifeFailed?.Invoke();
        }
    }
}
