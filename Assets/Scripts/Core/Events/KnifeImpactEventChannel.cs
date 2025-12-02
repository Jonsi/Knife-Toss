using System;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Events/Knife Impact Channel")]
    public class KnifeImpactEventChannel : ScriptableObject
    {
        public event Action KnifeStuck;
        public event Action<Vector3> KnifeStuckAt;
        public event Action KnifeFailed;
        public event Action<Vector3> KnifeFailedAt;

        public void RaiseKnifeStuck()
        {
            KnifeStuck?.Invoke();
        }

        public void RaiseKnifeStuck(Vector3 position)
        {
            KnifeStuckAt?.Invoke(position);
            KnifeStuck?.Invoke();
        }

        public void RaiseKnifeFailed()
        {
            KnifeFailed?.Invoke();
        }

        public void RaiseKnifeFailed(Vector3 position)
        {
            KnifeFailedAt?.Invoke(position);
            KnifeFailed?.Invoke();
        }
    }
}
