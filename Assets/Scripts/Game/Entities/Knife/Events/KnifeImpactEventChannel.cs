using Common.Events;
using Game.Entities.Knife.Data;
using UnityEngine;

namespace Game.Entities.Knife.Events
{
    /// <summary>
    /// Event channel for knife impact outcomes (stuck or bounced).
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeImpactEventChannel", menuName = "Knife Toss/Events/Gameplay/Knife Impact")]
    public class KnifeImpactEventChannel : EventChannel<KnifeImpactEventData>
    {
    }
}

