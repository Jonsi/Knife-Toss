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

        [Tooltip("Optional root for the stage reward chest UI.")]
        [SerializeField] private GameObject _RewardRoot;

        [Tooltip("Optional chest image tinted by reward state.")]
        [SerializeField] private Image _RewardChestImage;

        [Tooltip("Claim button shown for the stage reward.")]
        [SerializeField] private Button _ClaimButton;

        [Tooltip("Optional object enabled after the stage reward was claimed.")]
        [SerializeField] private GameObject _ClaimedStateRoot;

        [Tooltip("Sprite used for the chest once the reward was claimed.")]
        [SerializeField] private Sprite _ClaimedChestSprite;

        [Tooltip("Tint used when the stage reward cannot be claimed yet.")]
        [SerializeField] private Color _RewardUnavailableColor = new(0.55f, 0.55f, 0.55f, 1f);

        [Tooltip("Tint used when the stage reward is claimable or already claimed.")]
        [SerializeField] private Color _RewardAvailableColor = Color.white;

        private Sprite _DefaultChestSprite;

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

            RefreshRewardReferences();

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

            if (_RewardRoot)
            {
                _RewardRoot.SetActive(itemData.HasReward);
            }

            if (_RewardChestImage)
            {
                if (_DefaultChestSprite == null)
                {
                    _DefaultChestSprite = _RewardChestImage.sprite;
                }

                _RewardChestImage.sprite = itemData.IsRewardClaimed && _ClaimedChestSprite != null
                    ? _ClaimedChestSprite
                    : _DefaultChestSprite;
                _RewardChestImage.color = itemData.CanClaimReward || itemData.IsRewardClaimed
                    ? _RewardAvailableColor
                    : _RewardUnavailableColor;
            }

            if (_ClaimedStateRoot)
            {
                _ClaimedStateRoot.SetActive(itemData.HasReward && itemData.IsRewardClaimed);
            }

            if (_ClaimButton)
            {
                _ClaimButton.onClick.RemoveAllListeners();
                _ClaimButton.gameObject.SetActive(itemData.HasReward && !itemData.IsRewardClaimed);
                _ClaimButton.interactable = itemData.CanClaimReward;

                if (itemData.CanClaimReward)
                {
                    _ClaimButton.onClick.AddListener(() => onRewardClaimed?.Invoke(itemData.StageIndex));
                }
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

        private void RefreshRewardReferences()
        {
            _RewardRoot = ResolveGameObject(_RewardRoot, "RewardRoot");
            _RewardChestImage = ResolveComponent(_RewardChestImage, "RewardRoot/RewardChest");
            _ClaimButton = ResolveComponent(_ClaimButton, "RewardRoot/ClaimButton");
            _ClaimedStateRoot = ResolveGameObject(_ClaimedStateRoot, "RewardRoot/ClaimedText");
        }

        private GameObject ResolveGameObject(GameObject current, string childPath)
        {
            if (IsAlive(current))
            {
                return current;
            }

            Transform child = transform.Find(childPath);
            return child != null ? child.gameObject : null;
        }

        private T ResolveComponent<T>(T current, string childPath) where T : Component
        {
            if (IsAlive(current))
            {
                return current;
            }

            Transform child = transform.Find(childPath);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static bool IsAlive(UnityEngine.Object obj)
        {
            try
            {
                return obj != null;
            }
            catch (MissingReferenceException)
            {
                return false;
            }
        }
    }
}
