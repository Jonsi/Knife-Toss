using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Stages
{
    /// <summary>
    /// Represents a stage item in the stages selection list.
    /// </summary>
    public class UiStageItem : MonoBehaviour
    {
        [Tooltip("Image component to display the stage icon.")]
        [SerializeField] private Image _StageIconImage;

        [Tooltip("Text component to display the stage number.")]
        [SerializeField] private TMP_Text _StageNumberText;

        [Tooltip("Text component to display the stage name.")]
        [SerializeField] private TMP_Text _StageNameText;

        /// <summary>
        /// Initializes the stage item with data.
        /// </summary>
        /// <param name="icon">The icon to display.</param>
        /// <param name="stageNumber">The number of the stage.</param>
        /// <param name="stageName">The name of the stage.</param>
        public void Initialize(Sprite icon, int stageNumber, string stageName)
        {
             if (_StageIconImage != null) _StageIconImage.sprite = icon;
             if (_StageNumberText != null) _StageNumberText.text = stageNumber.ToString();
             if (_StageNameText != null) _StageNameText.text = stageName;
        }
    }
}
