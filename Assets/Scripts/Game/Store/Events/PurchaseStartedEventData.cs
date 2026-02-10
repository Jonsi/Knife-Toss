using Game.Store.Data;

namespace Game.Store.Events
{
    /// <summary>
    /// Event payload for purchase start.
    /// </summary>
    public readonly struct PurchaseStartedEventData
    {
        public PurchasableEntry Entry { get; }

        public PurchaseStartedEventData(PurchasableEntry entry)
        {
            Entry = entry;
        }
    }
}
