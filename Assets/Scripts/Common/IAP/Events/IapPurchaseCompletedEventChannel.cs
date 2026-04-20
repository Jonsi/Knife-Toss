using Common.Events;
using UnityEngine;

namespace Common.IAP.Events
{
    [CreateAssetMenu(fileName = "IapPurchaseCompletedEventChannel", menuName = "Knife Toss/Events/IAP/Purchase Completed")]
    public class IapPurchaseCompletedEventChannel : EventChannel<IapPurchaseEventData> { }
}
