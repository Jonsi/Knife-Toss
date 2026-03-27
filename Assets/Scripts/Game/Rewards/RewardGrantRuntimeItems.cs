using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.Persistence.Services;
using Game.Rewards.Configs;
using UnityEngine;

namespace Game.Rewards
{
    /// <summary>
    /// Runtime grant item for a reward entry backed by <see cref="Game.Items.Configs.ItemConfigurationBase"/>.
    /// </summary>
    public class ItemConfigRewardGrantItem : IRewardItem
    {
        private readonly RewardGrantEntry _Entry;

        /// <summary>
        /// Creates an item bound to the given grant asset.
        /// </summary>
        public ItemConfigRewardGrantItem(RewardGrantEntry entry)
        {
            _Entry = entry;
        }

        /// <inheritdoc/>
        public string DisplayText => _Entry.Item != null ? _Entry.Item.GetRewardDisplayText(_Entry.Count) : string.Empty;

        /// <inheritdoc/>
        public Sprite Icon =>
            _Entry.PresentationIconOverride != null
                ? _Entry.PresentationIconOverride
                : _Entry.Item != null ? _Entry.Item.GetItemIcon() : null;

        /// <inheritdoc/>
        public UniTask GrantAsync(IRewardGrantContext context) =>
            _Entry.Item != null
                ? _Entry.Item.GrantAsync(_Entry.Count, context)
                : UniTask.CompletedTask;
    }

    /// <summary>
    /// Legacy runtime item for the older currency reward grant entry type.
    /// </summary>
    public class CurrencyRewardGrantItem : IRewardItem
    {
        private readonly CurrencyRewardGrantEntry _Entry;

        public CurrencyRewardGrantItem(CurrencyRewardGrantEntry entry)
        {
            _Entry = entry;
        }

        public string DisplayText =>
            _Entry.CurrencyType == CurrencyType.None || _Entry.Amount < RewardGrantValidation.MinimumCurrencyOrPowerUpAmount
                ? string.Empty
                : $"+{_Entry.Amount} {_Entry.CurrencyType.ToDisplayLabel()}";

        public Sprite Icon => null;

        public UniTask GrantAsync(IRewardGrantContext context)
        {
            var currencyService = context.Resolve<ICurrencyService>();
            currencyService.AddBalance(_Entry.CurrencyType.ToTypeId(), _Entry.Amount);
            return UniTask.CompletedTask;
        }
    }

    /// <summary>
    /// Legacy runtime item for the older power-up reward grant entry type.
    /// </summary>
    public class PowerUpRewardGrantItem : IRewardItem
    {
        private readonly PowerUpRewardGrantEntry _Entry;

        public PowerUpRewardGrantItem(PowerUpRewardGrantEntry entry)
        {
            _Entry = entry;
        }

        public string DisplayText =>
            _Entry.PowerUp != null && _Entry.Count >= RewardGrantValidation.MinimumCurrencyOrPowerUpAmount
                ? _Entry.PowerUp.GetRewardDisplayText(_Entry.Count)
                : string.Empty;

        public Sprite Icon => _Entry.PowerUp != null ? _Entry.PowerUp.GetItemIcon() : null;

        public UniTask GrantAsync(IRewardGrantContext context) =>
            _Entry.PowerUp != null
                ? _Entry.PowerUp.GrantAsync(_Entry.Count, context)
                : UniTask.CompletedTask;
    }

    /// <summary>
    /// Legacy runtime item for the older knife skin reward grant entry type.
    /// </summary>
    public class KnifeSkinRewardGrantItem : IRewardItem
    {
        private readonly KnifeSkinRewardGrantEntry _Entry;

        public KnifeSkinRewardGrantItem(KnifeSkinRewardGrantEntry entry)
        {
            _Entry = entry;
        }

        public string DisplayText => _Entry.KnifeSkin != null ? _Entry.KnifeSkin.GetRewardDisplayText(1) : string.Empty;

        public Sprite Icon => _Entry.KnifeSkin != null ? _Entry.KnifeSkin.GetItemIcon() : null;

        public UniTask GrantAsync(IRewardGrantContext context) =>
            _Entry.KnifeSkin != null
                ? _Entry.KnifeSkin.GrantAsync(1, context)
                : UniTask.CompletedTask;
    }
}
