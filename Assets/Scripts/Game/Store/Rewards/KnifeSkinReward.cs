using System;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Persistence.Services;
using Game.Store.Configs;
using UnityEngine;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Grants a permanent knife skin unlock.
    /// </summary>
    [Serializable]
    public class KnifeSkinReward : ItemReward, IUniqueReward
    {
        [field: SerializeField]
        [field: Tooltip("Skin definition to unlock; persisted id comes from this asset.")]
        public KnifeSkinConfig Config { get; private set; }

        /// <inheritdoc/>
        public override Sprite Icon => Config != null ? Config.Icon : null;

        public string AlreadyOwnedReason => "Item already owned.";

        /// <inheritdoc/>
        public override string GetDisplayText() =>
            Config != null ? Config.name : string.Empty;

        public bool IsOwned(IRewardGrantContext context)
        {
            if (Config == null || string.IsNullOrEmpty(Config.Id))
            {
                return false;
            }

            var playerData = context.Resolve<PlayerDataService>();
            return playerData.EnsureKnifeSkinCollection().IsUnlocked(Config.Id);
        }

        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            if (Config == null || string.IsNullOrEmpty(Config.Id))
            {
                return UniTask.CompletedTask;
            }

            var playerData = context.Resolve<PlayerDataService>();
            var collection = playerData.EnsureKnifeSkinCollection();
            var didUnlock = collection.Unlock(Config.Id);

            if (didUnlock)
            {
                playerData.SaveKnifeSkinCollection();
            }

            return UniTask.CompletedTask;
        }
    }
}
