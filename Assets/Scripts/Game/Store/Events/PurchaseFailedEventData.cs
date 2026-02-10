using Game.Store.Data;

namespace Game.Store.Events
{
    /// <summary>
    /// Event payload for purchase failure.
    /// </summary>
    public readonly struct PurchaseFailedEventData
    {
        public PurchasableEntry Entry { get; }
        public string Reason { get; }

        public PurchaseFailedEventData(PurchasableEntry entry, string reason)
        {
            Entry = entry;
            Reason = reason;
        }
    }
}
