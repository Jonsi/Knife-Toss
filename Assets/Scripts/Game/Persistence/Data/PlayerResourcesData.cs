using System;
using System.Collections.Generic;
using Game.PowerUps.Data;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Container for player-owned resources: power-ups and currency balances.
    /// </summary>
    [Serializable]
    public class PlayerResourcesData
    {
        public PowerUpInventoryData PowerUps = new();
        public List<CurrencyBalanceEntry> Currencies = new();
    }
}
