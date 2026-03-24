using System;
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
        [Tooltip("Button component for interaction.")]
        [SerializeField] private Button _Button;

        [Tooltip("Image component to display the stage icon.")]
        [SerializeField] private Image _StageIconImage;

        [Tooltip("Text component to display the stage number.")]
        [SerializeField] private TMP_Text _StageNumberText;

        [Tooltip("Text component to display the stage name.")]
        [SerializeField] private TMP_Text _StageNameText;

        [Tooltip("Optional object enabled for locked stages.")]
        [SerializeField] private GameObject _LockedStateRoot;

        [Tooltip("Optional object enabled for the current stage.")]
        [SerializeField] private GameObject _CurrentStateRoot;

        /// <summary>
        /// Initializes the stage item with data.
        /// </summary>
        /// <param name="itemData">The item state to render.</param>
        /// <param name="onClicked">Callback when the item is clicked.</param>
        public void Initialize(StageListItemData itemData, Action<int> onClicked)
        {
            if (itemData == null)
            {
                return;
            }

            if (_StageIconImage != null)
            {
                _StageIconImage.sprite = itemData.Icon;
            }

            if (_StageNumberText != null)
            {
                _StageNumberText.text = itemData.StageNumber.ToString();
            }

            if (_StageNameText != null)
            {
                _StageNameText.text = itemData.StageName;
            }

            if (_LockedStateRoot != null)
            {
                _LockedStateRoot.SetActive(itemData.IsLocked);
            }

            if (_CurrentStateRoot != null)
            {
                _CurrentStateRoot.SetActive(itemData.IsCurrent);
            }

            if (_Button == null)
            {
                return;
            }

            _Button.onClick.RemoveAllListeners();
            _Button.interactable = !itemData.IsLocked;

            if (!itemData.IsLocked)
            {
                _Button.onClick.AddListener(() => onClicked?.Invoke(itemData.StageIndex));
            }
        }
    }
}
