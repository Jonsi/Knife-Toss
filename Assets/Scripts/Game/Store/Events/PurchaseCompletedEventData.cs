using Game.Store.Data;

namespace Game.Store.Events
{
    /// <summary>
    /// Event payload for purchase completion.
    /// </summary>
    public readonly struct PurchaseCompletedEventData
    {
        public PurchasableEntry Entry { get; }

        public PurchaseCompletedEventData(PurchasableEntry entry)
        {
            Entry = entry;
        }
    }
}
