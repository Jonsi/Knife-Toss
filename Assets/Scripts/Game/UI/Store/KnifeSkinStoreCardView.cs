using Game.Store.Data;
using Game.Store.Rewards;

namespace Game.UI.Store
{
    /// <summary>
    /// Card view used for knife skin store items.
    /// </summary>
    public class KnifeSkinStoreCardView : StoreCardView
    {
        protected override void BindEntry(PurchasableEntry entry)
        {
            if (entry?.Reward is KnifeSkinReward knifeSkinReward && knifeSkinReward.Config != null)
            {
                SetDisplaySprite(knifeSkinReward.Config.Icon);
            }
        }
    }
}
