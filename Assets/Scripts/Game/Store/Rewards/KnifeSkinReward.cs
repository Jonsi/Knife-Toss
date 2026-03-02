using System;
using Cysharp.Threading.Tasks;
using Game.Persistence.Services;
using Game.Store.Configs;
using UnityEngine;
using VContainer;

namespace Game.Store.Rewards
{
    /// <summary>
    /// Grants a permanent knife skin unlock.
    /// </summary>
    [Serializable]
    public class KnifeSkinReward : ItemReward, IUniqueReward
    {
        [field: SerializeField] public KnifeSkinConfig Config { get; private set; }
        [field: SerializeField] public bool AutoEquipOnPurchase { get; private set; }

        public string AlreadyOwnedReason => "Item already owned.";

        public bool IsOwned(IObjectResolver resolver)
        {
            if (Config == null || string.IsNullOrEmpty(Config.Id))
            {
                return false;
            }

            var playerData = resolver.Resolve<PlayerDataService>();
            return playerData.EnsureKnifeSkinCollection().IsUnlocked(Config.Id);
        }

        public override UniTask GrantAsync(IObjectResolver resolver)
        {
            if (Config == null || string.IsNullOrEmpty(Config.Id))
            {
                return UniTask.CompletedTask;
            }

            var playerData = resolver.Resolve<PlayerDataService>();
            var collection = playerData.EnsureKnifeSkinCollection();
            var didUnlock = collection.Unlock(Config.Id);

            if (didUnlock && AutoEquipOnPurchase)
            {
                collection.Equip(Config.Id);
            }

            if (didUnlock)
            {
                playerData.SaveKnifeSkinCollection();
            }

            return UniTask.CompletedTask;
        }
    }
}
