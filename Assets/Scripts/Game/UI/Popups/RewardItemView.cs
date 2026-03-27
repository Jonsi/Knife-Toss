using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Single reward card in the reward popup.
    /// </summary>
    public class RewardItemView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reward icon; disabled when the reward supplies no sprite.")]
        private Image _Icon;

        [SerializeField]
        [Tooltip("Primary amount or status text shown on the card.")]
        private TextMeshProUGUI _AmountLabel;

        /// <summary>
        /// Fills the card with the reward presentation data.
        /// </summary>
        /// <param name="icon">Optional icon; card hides the image when null.</param>
        /// <param name="displayText">Reward display text.</param>
        public void Setup(Sprite icon, string displayText)
        {
            if (_Icon != null)
            {
                _Icon.sprite = icon;
                _Icon.enabled = icon != null;
            }

            if (_AmountLabel != null)
            {
                _AmountLabel.text = ExtractPrimaryText(displayText);
            }
        }

        private static string ExtractPrimaryText(string displayText)
        {
            var value = string.IsNullOrWhiteSpace(displayText) ? string.Empty : displayText.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.StartsWith("MAX ", System.StringComparison.OrdinalIgnoreCase))
            {
                return value.ToUpperInvariant();
            }

            if (value[0] == '+')
            {
                var parts = value.Substring(1).Split(' ', 2);
                return parts.Length > 0 ? parts[0] : string.Empty;
            }

            return value.ToUpperInvariant();
        }
    }
}
