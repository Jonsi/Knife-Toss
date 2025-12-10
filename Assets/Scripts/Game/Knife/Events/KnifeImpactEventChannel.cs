using Common.Events;
using Game.Knife.Data;
using UnityEngine;

namespace Game.Knife.Events
{
    /// <summary>
    /// Event channel for knife impact outcomes (stuck or bounced).
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Events/Knife Impact Event Channel")]
    public class KnifeImpactEventChannel : EventChannel<KnifeImpactEventData>
    {
    }
}

