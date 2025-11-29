using System;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Events/Knife Impact Channel")]
    public class KnifeImpactEventChannel : ScriptableObject
    {
        public event Action KnifeStuck;
        public event Action KnifeFailed;

        public void RaiseKnifeStuck()
        {
            KnifeStuck?.Invoke();
        }

        public void RaiseKnifeFailed()
        {
            KnifeFailed?.Invoke();
        }
    }
}
