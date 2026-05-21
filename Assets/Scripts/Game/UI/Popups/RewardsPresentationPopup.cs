using System;
using System.Collections.Generic;
using System.Threading;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Orchestrates the rewards popup: chest interaction, staggered reward reveal, and continue dismissal.
    /// </summary>
    public class RewardsPresentationPopup : BasePopup
    {
        [SerializeField]
        private RewardsChestTapOpenView _ChestTapOpenView;

        [SerializeField]
        private StaggeredRewardRowsPresenter _RewardRowsPresenter;

        [SerializeField]
        private TextMeshProUGUI _BottomText;

        [SerializeField]
        private Button _BackgroundButton;

        [SerializeField]
        private string _TapToOpenLabel = "Tap To Open !";

        [SerializeField]
        private string _ContinueLabel = "Continue";

        [SerializeField]
        private Transform _ChestOpenEffectSpawnParent;

        private CancellationTokenSource _PresentationCts;

        /// <summary>
        /// Fills the chest reference when a nested prefab leaves the serialized link null (composition root only).
        /// </summary>
        internal void EnsureChestViewFromCompositionRoot(RewardsChestTapOpenView chestTapOpenView)
        {
            if (_ChestTapOpenView == null && chestTapOpenView != null)
            {
                _ChestTapOpenView = chestTapOpenView;
            }
        }

        /// <summary>
        /// Supplies sprites for closed and opened chest visuals (overrides inspector values when non-null).
        /// </summary>
        public void SetChestSprites(Sprite closedChest, Sprite openedChest)
        {
            if (_ChestTapOpenView == null)
            {
                return;
            }

            _ChestTapOpenView.ConfigureSprites(closedChest, openedChest);
        }

        /// <summary>
        /// Supplies reward entries shown after the chest opens.
        /// </summary>
        public void SetRewards(IReadOnlyList<IRewardItem> rewards)
        {
            _RewardRowsPresenter?.SetRewards(rewards);
        }

        public override void Show()
        {
            CancelPresentation();
            WireBackgroundDismiss(false);
            ApplyBottomPromptText(_TapToOpenLabel);
            SubscribeChestOpened(false);
            base.Show();
            _ChestTapOpenView?.SetOpenEffectSpawnParent(_ChestOpenEffectSpawnParent);
            _ChestTapOpenView?.ResetState();
            _RewardRowsPresenter?.Clear();
            SubscribeChestOpened(true);
        }

        private void OnDisable()
        {
            SubscribeChestOpened(false);
            WireBackgroundDismiss(false);
        }

        private void OnDestroy()
        {
            CancelPresentation();
            SubscribeChestOpened(false);
            WireBackgroundDismiss(false);
        }

        private void SubscribeChestOpened(bool subscribe)
        {
            if (_ChestTapOpenView == null)
            {
                return;
            }

            _ChestTapOpenView.Opened -= OnChestOpened;

            if (subscribe)
            {
                _ChestTapOpenView.Opened += OnChestOpened;
            }
        }

        private void OnChestOpened()
        {
            RunRewardsPresentation().Forget();
        }

        private async UniTaskVoid RunRewardsPresentation()
        {
            CancelPresentation();
            _PresentationCts = new CancellationTokenSource();
            var token = _PresentationCts.Token;

            try
            {
                if (_RewardRowsPresenter != null)
                {
                    await _RewardRowsPresenter.PresentAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            ApplyBottomPromptText(_ContinueLabel);
            WireBackgroundDismiss(true);
        }

        private void WireBackgroundDismiss(bool bind)
        {
            if (_BackgroundButton == null)
            {
                return;
            }

            _BackgroundButton.onClick.RemoveListener(OnBackgroundDismissClicked);

            if (bind)
            {
                _BackgroundButton.onClick.AddListener(OnBackgroundDismissClicked);
            }
        }

        private void ApplyBottomPromptText(string value)
        {
            if (_BottomText == null)
            {
                return;
            }

            _BottomText.text = value ?? string.Empty;
        }

        private void OnBackgroundDismissClicked()
        {
            WireBackgroundDismiss(false);
            Hide();
        }

        private void CancelPresentation()
        {
            if (_PresentationCts == null)
            {
                return;
            }

            _PresentationCts.Cancel();
            _PresentationCts.Dispose();
            _PresentationCts = null;
        }
    }
}
