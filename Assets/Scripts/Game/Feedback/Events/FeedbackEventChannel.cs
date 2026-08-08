using Common.Events;
using Game.Feedback.Data;
using UnityEngine;

namespace Game.Feedback.Events
{
    /// <summary>
    /// Shared typed channel for presentation-neutral feedback requests.
    /// Domain-to-feedback adapters raise this channel; domain services do not depend on presenters.
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackEventChannel", menuName = "Knife Toss/Events/Feedback/Feedback")]
    public class FeedbackEventChannel : EventChannel<FeedbackEventData>
    {
    }
}
