using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Event channel for hitable object impacts.
    /// </summary>
    [CreateAssetMenu(fileName = "HitableObjectHitEventChannel", menuName = "Knife Toss/Configurations/Events/Hitable Object Hit Channel")]
    public sealed class HitableObjectHitEventChannel : ScriptableObject
    {
        public event Action<Vector3> HitableHitAt;

        /// <summary>
        /// Performs the RaiseHit operation.
        /// </summary>
        public void RaiseHit(Vector3 position)
        {
            HitableHitAt?.Invoke(position);
        }
    }
}
