using KnifeToss.Core.Data.Knife;
using UnityEngine;

namespace Infrastructure.Events
{
    /// <summary>
    /// Event channel for knife impact outcomes (stuck or bounced).
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Events/Knife Impact Event Channel")]
    public class KnifeImpactEventChannel : EventChannel<KnifeImpactEventData>
    {
    }
}

