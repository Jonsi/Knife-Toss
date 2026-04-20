using Common.Events;
using UnityEngine;

namespace Common.IAP.Events
{
    [CreateAssetMenu(fileName = "IapPurchaseRestoredEventChannel", menuName = "Knife Toss/Events/IAP/Purchase Restored")]
    public class IapPurchaseRestoredEventChannel : EventChannel<IapPurchaseEventData> { }
}
