using Game.Presentation.Common;
using TMPro;
using UnityEngine;

namespace Game.Presentation.HUD.Collectibles
{
    /// <summary>
    /// Displays the total collectible count.
    /// </summary>
    public class CollectibleCountView : BaseView<CollectibleCountModel>
    {
        [SerializeField] private TMP_Text _CountText;

        public void SetCollectibleCount(int totalCollectibles)
        {
            if (_CountText == null)
            {
                return;
            }

            _CountText.text = totalCollectibles.ToString();
        }
    }
}
