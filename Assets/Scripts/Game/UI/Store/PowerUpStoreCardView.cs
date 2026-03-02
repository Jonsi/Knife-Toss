using Game.Store.Data;
using Game.Store.Rewards;
using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Card view used for power-up store items.
    /// </summary>
    public class PowerUpStoreCardView : StoreCardView
    {
        [field: SerializeField] public TMP_Text RewardAmountText { get; private set; }

        protected override void BindEntry(PurchasableEntry entry)
        {
            if (entry?.Reward is PowerUpReward powerUpReward && powerUpReward.Config != null)
            {
                SetDisplaySprite(powerUpReward.Config.Icon);

                if (RewardAmountText != null)
                {
                    RewardAmountText.text = $"+{powerUpReward.Quantity}";
                }
            }
        }
    }
}
