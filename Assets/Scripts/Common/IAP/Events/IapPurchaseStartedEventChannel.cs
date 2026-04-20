using Common.Events;
using UnityEngine;

namespace Common.IAP.Events
{
    [CreateAssetMenu(fileName = "IapPurchaseStartedEventChannel", menuName = "Knife Toss/Events/IAP/Purchase Started")]
    public class IapPurchaseStartedEventChannel : EventChannel<IapPurchaseEventData> { }
}
