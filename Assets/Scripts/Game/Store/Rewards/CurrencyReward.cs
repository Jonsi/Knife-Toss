using System;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Economy.Interfaces;
using UnityEngine;
using VContainer;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Grants currency to the player's balance.
    /// </summary>
    [Serializable]
    public class CurrencyReward : ItemReward
    {
        [SerializeField] private CurrencyType _CurrencyType;
        [SerializeField] private int _Amount;

        public override UniTask GrantAsync(IObjectResolver resolver)
        {
            if (_CurrencyType == CurrencyType.None || _Amount <= 0)
            {
                return UniTask.CompletedTask;
            }

            var currencyService = resolver.Resolve<ICurrencyService>();
            currencyService.AddBalance(_CurrencyType.ToTypeId(), _Amount);
            return UniTask.CompletedTask;
        }
    }
}
