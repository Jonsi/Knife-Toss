using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Serializable entry for a single currency balance (type and amount).
    /// </summary>
    [Serializable]
    public class CurrencyBalanceEntry
    {
        public string TypeId;
        public int Balance;
    }
}
