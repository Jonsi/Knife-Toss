using System;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private string _CurrencyId;
        [SerializeField] private int _Amount;

        public override UniTask GrantAsync(IObjectResolver resolver)
        {
            if (string.IsNullOrEmpty(_CurrencyId) || _Amount <= 0)
            {
                return UniTask.CompletedTask;
            }

            var currencyService = resolver.Resolve<ICurrencyService>();
            currencyService.AddBalance(_CurrencyId, _Amount);
            return UniTask.CompletedTask;
        }
    }
}
