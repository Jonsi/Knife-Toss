using Game.Store.Data;
using Game.Store.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Card view used for power-up store items.
    /// </summary>
    public class PowerUpStoreCardView : StoreCardView
    {
        [field: SerializeField] public TMP_Text RewardAmountText { get; private set; }
        [field: SerializeField] public TMP_Text CurrentAmountText { get; private set; }
        [field: SerializeField] public TMP_Text GrantAmountText { get; private set; }
        [field: SerializeField] public Image GrantIconImage { get; private set; }

        protected override void BindEntry(PurchasableEntry entry)
        {
            if (entry?.Reward is PowerUpReward powerUpReward && powerUpReward.Config != null)
            {
                SetDisplaySprite(powerUpReward.Config.Icon);

                if (RewardAmountText != null)
                {
                    RewardAmountText.text = powerUpReward.Config.DisplayName?.ToUpperInvariant();
                }

                if (GrantAmountText != null)
                {
                    GrantAmountText.text = $"+{powerUpReward.Quantity}";
                }

                if (GrantIconImage != null)
                {
                    GrantIconImage.sprite = powerUpReward.Config.Icon;
                    GrantIconImage.preserveAspect = true;
                }
            }
        }

        public void SetCurrentAmount(int amount)
        {
            if (CurrentAmountText != null)
            {
                CurrentAmountText.text = $"OWNED {amount}";
            }
        }
    }
}
