using TMPro;
using UnityEngine;

namespace Presentation.UI
{
    /// <summary>
    /// Displays the total fruit count with text.
    /// </summary>
    public sealed class FruitCountDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countText;

        /// <summary>
        /// Updates the display with the current fruit count.
        /// </summary>
        public void Render(int totalFruitsHit)
        {
            if (countText != null)
            {
                countText.text = totalFruitsHit.ToString();
            }
        }
    }
}

