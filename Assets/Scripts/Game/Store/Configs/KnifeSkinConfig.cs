using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Items.Configs;
using Game.Persistence.Services;
using UnityEngine;

namespace Game.Store.Configs
{
    /// <summary>
    /// Definition asset for a knife skin.
    /// Only the stable Id is persisted in player data.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSkinConfig", menuName = "Knife Toss/Systems/Store/Knife Skin Config")]
    public class KnifeSkinConfig : ItemConfigurationBase
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public Sprite KnifeSprite { get; private set; }

        /// <inheritdoc/>
        public override string GetItemDisplayName() => string.IsNullOrEmpty(name) ? "Knife Skin" : name;

        /// <inheritdoc/>
        public override Sprite GetItemIcon() => Icon;

        /// <inheritdoc/>
        public override UniTask GrantAsync(int count, IRewardGrantContext context)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return UniTask.CompletedTask;
            }

            var playerData = context.Resolve<PlayerDataService>();
            var collection = playerData.EnsureKnifeSkinCollection();
            if (collection.Unlock(Id))
            {
                playerData.SaveKnifeSkinCollection();
            }

            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override string GetRewardDisplayText(int count) => GetItemDisplayName();

        public Sprite GetKnifeSprite()
        {
            return KnifeSprite != null ? KnifeSprite : Icon;
        }
    }
}
