namespace Game.Economy.Events
{
    /// <summary>
    /// Payload for currency balance change events.
    /// </summary>
    public struct CurrencyBalanceChangedEventData
    {
        public string CurrencyId;
        public int PreviousBalance;
        public int NewBalance;
        public int AmountChanged;
    }
}
