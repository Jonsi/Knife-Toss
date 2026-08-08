using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Modal that lists granted rewards and completes when the player taps Collect.
    /// </summary>
    public class RewardPopup : BasePopup, IRewardPresenter
    {
        [SerializeField]
        [Tooltip("Header line for the reward flow (e.g. level cleared, stage complete).")]
        private TextMeshProUGUI _TitleText;

        [SerializeField]
        [Tooltip("Parent for dynamically spawned reward rows (typically under a Vertical Layout Group).")]
        private Transform _RewardItemsRoot;

        [SerializeField]
        [Tooltip("Prefab for one reward row (icon + label). Instantiated per granted reward.")]
        private RewardItemView _RewardItemPrefab;

        [SerializeField]
        [Tooltip("Closes the popup and completes the presentation task when clicked.")]
        private Button _CollectButton;

        private UniTaskCompletionSource _Completion;

        /// <inheritdoc/>
        public async UniTask PresentAsync(string title, IReadOnlyList<IRewardItem> rewards)
        {
            if (_TitleText != null)
            {
                _TitleText.text = title ?? string.Empty;
            }

            ClearRewardItems();

            if (rewards != null && _RewardItemPrefab != null && _RewardItemsRoot != null)
            {
                foreach (var reward in rewards)
                {
                    if (reward == null)
                    {
                        continue;
                    }

                    var view = Object.Instantiate(_RewardItemPrefab, _RewardItemsRoot);
                    view.Setup(reward.Icon, reward.DisplayText);
                }
            }

            _Completion = new UniTaskCompletionSource();

            if (_CollectButton != null)
            {
                ActionButtonPresenter.GetOrAdd(_CollectButton);
                _CollectButton.onClick.RemoveListener(OnCollectClicked);
                _CollectButton.onClick.AddListener(OnCollectClicked);
            }

            Show();

            if (_CollectButton == null)
            {
                _Completion.TrySetResult();
            }

            await _Completion.Task;

            if (_CollectButton != null)
            {
                _CollectButton.onClick.RemoveListener(OnCollectClicked);
            }

            Hide();
        }

        private void OnCollectClicked()
        {
            _Completion?.TrySetResult();
        }

        private void ClearRewardItems()
        {
            if (_RewardItemsRoot == null)
            {
                return;
            }

            for (var i = _RewardItemsRoot.childCount - 1; i >= 0; i--)
            {
                var child = _RewardItemsRoot.GetChild(i);
                Object.Destroy(child.gameObject);
            }
        }
    }
}
