using Common.Events;
using UnityEngine;

namespace Common.Rewards.Events
{
    /// <summary>
    /// Raised after rewards are successfully granted by the reward grant service.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardGrantedEventChannel", menuName = "Knife Toss/Events/Rewards/Reward Granted")]
    public class RewardGrantedEventChannel : EventChannel<RewardGrantedEventData>
    {
    }
}
