using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Represents the KnifeImpactEventChannel class.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Configurations/Events/Knife Impact Channel")]
    public class KnifeImpactEventChannel : ScriptableObject
    {
        public event Action KnifeStuck;
        public event Action<Vector3> KnifeStuckAt;

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

    }
}
