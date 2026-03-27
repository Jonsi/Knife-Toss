using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Items.Configs
{
    /// <summary>
    /// Base configuration for anything the player can collect, use, or unlock.
    /// Reward flows reference these configs directly instead of duplicating item data.
    /// </summary>
    public abstract class ItemConfigurationBase : ScriptableObject
    {
        /// <summary>
        /// Display name used in UI and reward presentation.
        /// </summary>
        public abstract string GetItemDisplayName();

        /// <summary>
        /// Icon used in UI and reward presentation.
        /// </summary>
        public abstract Sprite GetItemIcon();

        /// <summary>
        /// Applies this item grant to the player.
        /// </summary>
        public abstract UniTask GrantAsync(int count, IRewardGrantContext context);

        /// <summary>
        /// Whether the given reward count is valid for this item.
        /// </summary>
        public virtual bool IsValidRewardCount(int count) => count > 0;

        /// <summary>
        /// Human-readable reward label for popups.
        /// </summary>
        public virtual string GetRewardDisplayText(int count)
        {
            var itemName = GetItemDisplayName();
            if (string.IsNullOrEmpty(itemName) || !IsValidRewardCount(count))
            {
                return string.Empty;
            }

            return count == 1 ? itemName : $"+{count} {itemName}";
        }
    }
}
