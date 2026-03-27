using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.Persistence.Services;
using Game.Rewards.Configs;
using Game.Store.Rewards;
using UnityEngine;

namespace Game.Rewards
{
    /// <summary>
    /// Runtime grant item for <see cref="CurrencyRewardGrantEntry"/>.
    /// </summary>
    public class CurrencyRewardGrantItem : IRewardItem
    {
        private readonly CurrencyRewardGrantEntry _Entry;

        /// <summary>
        /// Creates an item bound to the given grant asset.
        /// </summary>
        public CurrencyRewardGrantItem(CurrencyRewardGrantEntry entry)
        {
            _Entry = entry;
        }

        /// <inheritdoc/>
        public string DisplayText =>
            _Entry.CurrencyType == CurrencyType.None || _Entry.Amount < RewardGrantValidation.MinimumCurrencyOrPowerUpAmount
                ? string.Empty
                : $"+{_Entry.Amount} {_Entry.CurrencyType.ToDisplayLabel()}";

        /// <inheritdoc/>
        public Sprite Icon => null;

        /// <inheritdoc/>
        public UniTask GrantAsync(IRewardGrantContext context)
        {
            var currencyService = context.Resolve<ICurrencyService>();
            currencyService.AddBalance(_Entry.CurrencyType.ToTypeId(), _Entry.Amount);
            return UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// Runtime grant item for <see cref="PowerUpRewardGrantEntry"/>.
    /// </summary>
    public class PowerUpRewardGrantItem : IRewardItem
    {
        private readonly PowerUpRewardGrantEntry _Entry;

        /// <summary>
        /// Creates an item bound to the given grant asset.
        /// </summary>
        public PowerUpRewardGrantItem(PowerUpRewardGrantEntry entry)
        {
            _Entry = entry;
        }

        /// <inheritdoc/>
        public string DisplayText =>
            _Entry.PowerUp != null && _Entry.Count >= RewardGrantValidation.MinimumCurrencyOrPowerUpAmount
                ? $"+{_Entry.Count} {_Entry.PowerUp.DisplayName}"
                : string.Empty;

        /// <inheritdoc/>
        public Sprite Icon => _Entry.PowerUp != null ? _Entry.PowerUp.Icon : null;

        /// <inheritdoc/>
        public UniTask GrantAsync(IRewardGrantContext context)
        {
            var playerData = context.Resolve<PlayerDataService>();
            playerData.EnsurePowerUpInventory().IncrementCount(_Entry.PowerUp.InventoryKey, _Entry.Count);
            playerData.SavePowerUpInventory();
            return UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// Runtime grant item for <see cref="KnifeSkinRewardGrantEntry"/>.
    /// </summary>
    public class KnifeSkinRewardGrantItem : IRewardItem, IUniqueReward
    {
        private readonly KnifeSkinRewardGrantEntry _Entry;

        /// <summary>
        /// Creates an item bound to the given grant asset.
        /// </summary>
        public KnifeSkinRewardGrantItem(KnifeSkinRewardGrantEntry entry)
        {
            _Entry = entry;
        }

        /// <inheritdoc/>
        public string AlreadyOwnedReason => "Item already owned.";

        /// <inheritdoc/>
        public string DisplayText => _Entry.KnifeSkin != null ? _Entry.KnifeSkin.name : string.Empty;

        /// <inheritdoc/>
        public Sprite Icon => _Entry.KnifeSkin != null ? _Entry.KnifeSkin.Icon : null;

        /// <inheritdoc/>
        public UniTask GrantAsync(IRewardGrantContext context)
        {
            var playerData = context.Resolve<PlayerDataService>();
            var collection = playerData.EnsureKnifeSkinCollection();
            if (collection.Unlock(_Entry.KnifeSkin.Id))
            {
                playerData.SaveKnifeSkinCollection();
            }

            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public bool IsOwned(IRewardGrantContext context)
        {
            if (_Entry.KnifeSkin == null || string.IsNullOrEmpty(_Entry.KnifeSkin.Id))
            {
                return false;
            }

            var playerData = context.Resolve<PlayerDataService>();
            return playerData.EnsureKnifeSkinCollection().IsUnlocked(_Entry.KnifeSkin.Id);
        }
    }
}
