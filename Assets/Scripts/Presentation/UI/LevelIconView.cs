using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Represents a reusable level icon view that can display an icon and level number.
    /// </summary>
    public sealed class LevelIconView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Color activeLevelColor = Color.white;
        [SerializeField] private Color inactiveLevelColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private TextMeshProUGUI levelNumberText;

        public bool IsActive { get; private set; }
        public int LevelNumber { get; private set; }

        /// <summary>
        /// Sets the sprite used for the level icon.
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (icon == null)
            {
                return;
            }

            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        /// <summary>
        /// Updates the level number label.
        /// </summary>
        public void SetLevelNumber(int levelNumber)
        {
            if (levelNumberText == null)
            {
                return;
            }

            levelNumberText.text = levelNumber.ToString();
            LevelNumber = levelNumber;
        }

        /// <summary>
        /// Applies unlocked/locked visuals to the level icon.
        /// </summary>
        public void SetLevelActive(bool isActive)
        {
            IsActive = isActive;
            Color targetColor = isActive ? activeLevelColor : inactiveLevelColor;

            if (icon != null)
            {
                icon.color = targetColor;
            }

            if (levelNumberText != null)
            {
                levelNumberText.color = targetColor;
            }
        }
    }
}
