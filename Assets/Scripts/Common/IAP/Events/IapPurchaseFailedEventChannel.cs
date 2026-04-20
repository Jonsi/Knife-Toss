using Common.Events;
using UnityEngine;

namespace Common.IAP.Events
{
    [CreateAssetMenu(fileName = "IapPurchaseFailedEventChannel", menuName = "Knife Toss/Events/IAP/Purchase Failed")]
    public class IapPurchaseFailedEventChannel : EventChannel<IapPurchaseEventData> { }
}
