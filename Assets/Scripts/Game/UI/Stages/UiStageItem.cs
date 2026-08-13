using System;
using Game.UI.Common;
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
        private const string ClaimActionLabel = "CLAIM";
        private const string ClaimedStatusLabel = "CLAIMED";

        [Tooltip("Button component for interaction.")]
        [SerializeField] private Button _Button;

        [Tooltip("Image component to display the stage thumbnail.")]
        [SerializeField] private Image _StageIconImage;

        [Tooltip("Text component to display the stage name.")]
        [SerializeField] private TMP_Text _StageNameText;

        [Tooltip("Optional object enabled for locked stages.")]
        [SerializeField] private GameObject _LockedStateRoot;

        [Tooltip("Shown when the stage has been cleared (beaten).")]
        [SerializeField] private GameObject _LevelCompleteIndicator;

        [Tooltip("Claim button shown when the stage has a configured reward.")]
        [SerializeField] private Button _ClaimButton;

        [Tooltip("Label on the claim button (e.g. CLAIM / CLAIMED).")]
        [SerializeField] private TMP_Text _ClaimButtonText;

        [Tooltip("Sprite for the claim button when the reward can be claimed.")]
        [SerializeField] private Sprite _ClaimButtonActiveSprite;

        [Tooltip("Sprite for the claim button when the reward cannot be claimed yet.")]
        [SerializeField] private Sprite _ClaimButtonInactiveSprite;

        [Tooltip("Sprite when the reward was already claimed; falls back to active sprite if not set.")]
        [SerializeField] private Sprite _ClaimButtonClaimedSprite;

        [Header("Reference Stage Tokens")]
        [SerializeField] private Sprite _CompleteTokenSprite;
        [SerializeField] private Sprite _CurrentTokenSprite;
        [SerializeField] private Sprite _LockedTokenSprite;

        private const string CompleteStatusLabel = "COMPLETE";
        private const string CurrentStatusLabel = "CURRENT";
        private const string LockedStatusLabel = "LOCKED";

        /// <summary>
        /// Initializes the stage item with data.
        /// </summary>
        /// <param name="itemData">The item state to render.</param>
        /// <param name="onClicked">Callback when the item is clicked.</param>
        /// <param name="onRewardClaimed">Callback when the reward claim button is clicked.</param>
        public void Initialize(StageListItemData itemData, Action<int> onClicked, Action<int> onRewardClaimed)
        {
            if (itemData == null)
            {
                return;
            }

            if (_StageIconImage != null)
            {
                Sprite stateToken = ResolveStateToken(itemData);
                _StageIconImage.sprite = stateToken != null ? stateToken : itemData.Icon;
                _StageIconImage.preserveAspect = true;
            }

            if (_StageNameText != null)
            {
                _StageNameText.text = itemData.StageNumber.ToString();
            }

            if (_LockedStateRoot != null)
            {
                _LockedStateRoot.SetActive(false);
            }

            if (_LevelCompleteIndicator != null)
            {
                _LevelCompleteIndicator.SetActive(false);
            }

            BindClaimButton(itemData, onRewardClaimed);

            ActionButtonPresenter.GetOrAdd(_Button);
            ActionButtonPresenter.GetOrAdd(_ClaimButton);

            if (_Button == null)
            {
                return;
            }

            _Button.onClick.RemoveAllListeners();
            _Button.interactable = !itemData.IsLocked;
            _Button.transition = Selectable.Transition.None;

            if (!itemData.IsLocked)
            {
                _Button.onClick.AddListener(() => onClicked?.Invoke(itemData.StageIndex));
            }
        }

        private Sprite ResolveStateToken(StageListItemData itemData)
        {
            if (itemData.IsLocked)
            {
                return _LockedTokenSprite;
            }

            if (itemData.IsCurrent)
            {
                return _CurrentTokenSprite;
            }

            return itemData.IsStageComplete ? _CompleteTokenSprite : _CurrentTokenSprite;
        }

        private void BindClaimButton(StageListItemData itemData, Action<int> onRewardClaimed)
        {
            if (_ClaimButton == null)
            {
                return;
            }

            _ClaimButton.onClick.RemoveAllListeners();

            if (!itemData.HasReward)
            {
                _ClaimButton.gameObject.SetActive(true);
                _ClaimButton.interactable = false;
                var statusGraphic = _ClaimButton.targetGraphic as Image;
                if (statusGraphic != null)
                {
                    statusGraphic.enabled = true;
                    statusGraphic.sprite = _ClaimButtonInactiveSprite;
                    statusGraphic.color = itemData.IsLocked
                        ? new Color32(50, 31, 14, 230)
                        : new Color32(1, 79, 67, 242);
                }
                SetClaimButtonText(GetStatusLabel(itemData));
                return;
            }

            _ClaimButton.gameObject.SetActive(true);
            var claimGraphic = _ClaimButton.targetGraphic as Image;
            if (claimGraphic != null)
            {
                claimGraphic.enabled = true;
            }

            if (!itemData.CanClaimReward && !itemData.IsRewardClaimed)
            {
                _ClaimButton.interactable = false;
                if (claimGraphic != null)
                {
                    claimGraphic.enabled = true;
                    claimGraphic.sprite = _ClaimButtonInactiveSprite;
                    claimGraphic.color = itemData.IsLocked
                        ? new Color32(50, 31, 14, 230)
                        : new Color32(1, 79, 67, 242);
                }
                SetClaimButtonText(GetStatusLabel(itemData));
                return;
            }

            if (itemData.IsRewardClaimed)
            {
                _ClaimButton.interactable = false;
                ApplyClaimVisual(claimGraphic, ClaimedSpriteOrFallback());
                SetClaimButtonText(ClaimedStatusLabel);
                return;
            }

            if (itemData.CanClaimReward)
            {
                _ClaimButton.interactable = true;
                ApplyClaimVisual(claimGraphic, _ClaimButtonActiveSprite);
                SetClaimButtonText(ClaimActionLabel);
                _ClaimButton.onClick.AddListener(() => onRewardClaimed?.Invoke(itemData.StageIndex));
                return;
            }

            _ClaimButton.interactable = false;
            ApplyClaimVisual(claimGraphic, _ClaimButtonInactiveSprite);
            SetClaimButtonText(ClaimActionLabel);
        }

        private static string GetStatusLabel(StageListItemData itemData)
        {
            if (itemData.IsLocked)
            {
                return LockedStatusLabel;
            }

            if (itemData.IsCurrent)
            {
                return CurrentStatusLabel;
            }

            return itemData.IsStageComplete ? CompleteStatusLabel : CurrentStatusLabel;
        }

        private Sprite ClaimedSpriteOrFallback()
        {
            return _ClaimButtonClaimedSprite != null ? _ClaimButtonClaimedSprite : _ClaimButtonActiveSprite;
        }

        private void ApplyClaimVisual(Image claimGraphic, Sprite sprite)
        {
            if (claimGraphic != null && sprite != null)
            {
                claimGraphic.sprite = sprite;
            }
        }

        private void SetClaimButtonText(string value)
        {
            if (_ClaimButtonText != null)
            {
                _ClaimButtonText.text = value;
            }
        }
    }
}
