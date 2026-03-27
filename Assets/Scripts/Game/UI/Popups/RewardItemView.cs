using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Single row in the reward popup (icon + label).
    /// </summary>
    public class RewardItemView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reward icon; disabled when the reward supplies no sprite.")]
        private Image _Icon;

        [SerializeField]
        [Tooltip("Short description text (e.g. +50 COIN).")]
        private TextMeshProUGUI _Label;

        /// <summary>
        /// Fills the row with the reward presentation data.
        /// </summary>
        /// <param name="icon">Optional icon; row hides the image when null.</param>
        /// <param name="displayText">Label text.</param>
        public void Setup(Sprite icon, string displayText)
        {
            if (_Label != null)
            {
                _Label.text = displayText ?? string.Empty;
            }

            if (_Icon != null)
            {
                _Icon.sprite = icon;
                _Icon.enabled = icon != null;
            }
        }
    }
}
