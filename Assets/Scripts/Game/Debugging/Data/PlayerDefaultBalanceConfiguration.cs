using System;
using System.Collections.Generic;
using Game.Economy.Data;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using UnityEngine;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Configurable resource defaults applied by debug reset cheats.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerDefaultBalanceConfiguration", menuName = "Knife Toss/Debug/Player Default Balance")]
    public class PlayerDefaultBalanceConfiguration : ScriptableObject
    {
        [SerializeField] private int _CoinBalance = 100;
        [SerializeField] private List<PowerUpDefaultBalance> _PowerUps = new List<PowerUpDefaultBalance>();

        public int CoinBalance => Mathf.Max(0, _CoinBalance);
        public IReadOnlyList<PowerUpDefaultBalance> PowerUps => _PowerUps;

        public void ApplyTo(PlayerData playerData)
        {
            if (playerData == null)
            {
                return;
            }

            playerData.Resources ??= new PlayerResourcesData();
            playerData.Resources.Currencies ??= new List<CurrencyBalanceEntry>();
            playerData.Resources.PowerUps ??= new PowerUpInventoryData();

            SetCurrency(playerData.Resources.Currencies, CurrencyType.Coin.ToTypeId(), CoinBalance);

            if (_PowerUps == null)
            {
                return;
            }

            for (int i = 0; i < _PowerUps.Count; i++)
            {
                PowerUpDefaultBalance entry = _PowerUps[i];
                if (entry?.Config == null || string.IsNullOrEmpty(entry.Config.InventoryKey))
                {
                    continue;
                }

                playerData.Resources.PowerUps.SetCount(entry.Config.InventoryKey, entry.Count);
            }
        }

        private static void SetCurrency(List<CurrencyBalanceEntry> currencies, string typeId, int balance)
        {
            if (currencies == null || string.IsNullOrEmpty(typeId))
            {
                return;
            }

            CurrencyBalanceEntry entry = currencies.Find(item => item != null && item.TypeId == typeId);
            if (entry == null)
            {
                currencies.Add(new CurrencyBalanceEntry
                {
                    TypeId = typeId,
                    Balance = Mathf.Max(0, balance)
                });
                return;
            }

            entry.Balance = Mathf.Max(0, balance);
        }
    }

    [Serializable]
    public class PowerUpDefaultBalance
    {
        [field: SerializeField] public PowerUpConfigBase Config { get; private set; }
        [field: SerializeField] public int Count { get; private set; } = 3;
    }
}
