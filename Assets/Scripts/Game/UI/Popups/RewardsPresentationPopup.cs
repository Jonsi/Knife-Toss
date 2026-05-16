using System;
using System.Collections.Generic;
using System.Threading;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.UI.Common;
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
        private Button _ContinueButton;

        private CancellationTokenSource _PresentationCts;

        /// <summary>
        /// Supplies sprites for closed and opened chest visuals (overrides inspector values when non-null).
        /// </summary>
        public void SetChestSprites(Sprite closedChest, Sprite openedChest)
        {
            if (_ChestTapOpenView != null)
            {
                _ChestTapOpenView.ConfigureSprites(closedChest, openedChest);
            }
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
            WireContinue(false);
            SetContinueVisible(false);
            SubscribeChestOpened(false);
            base.Show();
            _ChestTapOpenView?.ResetState();
            _RewardRowsPresenter?.Clear();
            SubscribeChestOpened(true);
        }

        private void OnDisable()
        {
            SubscribeChestOpened(false);
        }

        private void OnDestroy()
        {
            CancelPresentation();
            SubscribeChestOpened(false);

            if (_ContinueButton != null)
            {
                _ContinueButton.onClick.RemoveListener(OnContinueClicked);
            }
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

            SetContinueVisible(true);
            WireContinue(true);
        }

        private void WireContinue(bool bind)
        {
            if (_ContinueButton == null)
            {
                return;
            }

            _ContinueButton.onClick.RemoveListener(OnContinueClicked);

            if (bind)
            {
                _ContinueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void SetContinueVisible(bool visible)
        {
            if (_ContinueButton == null)
            {
                return;
            }

            _ContinueButton.gameObject.SetActive(visible);
        }

        private void OnContinueClicked()
        {
            WireContinue(false);
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

    /// <summary>
    /// Handles chest tap input, DOTween punch feedback, and switching to the opened visual state.
    /// </summary>
    public class RewardsChestTapOpenView : MonoBehaviour
    {
        private static readonly Vector3 DefaultTapPunchScale = new Vector3(0.12f, 0.12f, 0f);

        [SerializeField]
        private Image _ChestImage;

        [SerializeField]
        private Sprite _ClosedChestSprite;

        [SerializeField]
        private Sprite _OpenedChestSprite;

        [SerializeField]
        private RectTransform _ChestRectTransform;

        [SerializeField]
        private Button _TapChestButton;

        [SerializeField]
        private Transform _OpenEffectRoot;

        [SerializeField]
        [Min(1)]
        private int _TapsToOpen = 3;

        [SerializeField]
        private Vector3 _TapPunchScale = DefaultTapPunchScale;

        [SerializeField]
        [Min(0f)]
        private float _TapPunchDurationSeconds = 0.18f;

        [SerializeField]
        [Min(1)]
        private int _TapPunchVibrato = 6;

        [SerializeField]
        [Range(0f, 1f)]
        private float _TapPunchElasticity = 0.4f;

        [SerializeField]
        private bool _UseUnscaledTime = true;

        private int _TapCount;
        private bool _IsOpened;

        /// <summary>
        /// Raised once after the last required tap, when open visuals are applied.
        /// </summary>
        public event Action Opened;

        /// <summary>
        /// Updates chest sprites from code; non-null arguments override serialized defaults.
        /// </summary>
        public void ConfigureSprites(Sprite closedChest, Sprite openedChest)
        {
            if (closedChest != null)
            {
                _ClosedChestSprite = closedChest;
            }

            if (openedChest != null)
            {
                _OpenedChestSprite = openedChest;
            }

            ApplyClosedVisual();
        }

        /// <summary>
        /// Resets taps and closed visuals so the chest can be opened again.
        /// </summary>
        public void ResetState()
        {
            _TapCount = 0;
            _IsOpened = false;

            if (_OpenEffectRoot != null)
            {
                _OpenEffectRoot.gameObject.SetActive(false);
            }

            if (_TapChestButton != null)
            {
                _TapChestButton.interactable = true;
            }

            ApplyClosedVisual();
        }

        private void Awake()
        {
            if (_TapChestButton != null)
            {
                _TapChestButton.onClick.AddListener(OnChestTapped);
            }
        }

        private void OnDestroy()
        {
            if (_TapChestButton != null)
            {
                _TapChestButton.onClick.RemoveListener(OnChestTapped);
            }

            if (_ChestRectTransform != null)
            {
                DOTween.Kill(_ChestRectTransform);
            }
        }

        private void OnChestTapped()
        {
            if (_IsOpened)
            {
                return;
            }

            PlayTapPunch();
            _TapCount++;

            if (_TapCount < _TapsToOpen)
            {
                return;
            }

            OpenChest();
        }

        private void PlayTapPunch()
        {
            if (_ChestRectTransform == null)
            {
                return;
            }

            DOTween.Kill(_ChestRectTransform);
            _ChestRectTransform
                .DOPunchScale(_TapPunchScale, _TapPunchDurationSeconds, _TapPunchVibrato, _TapPunchElasticity)
                .SetUpdate(_UseUnscaledTime)
                .SetTarget(_ChestRectTransform);
        }

        private void OpenChest()
        {
            _IsOpened = true;

            if (_TapChestButton != null)
            {
                _TapChestButton.interactable = false;
            }

            if (_ChestImage != null && _OpenedChestSprite != null)
            {
                _ChestImage.sprite = _OpenedChestSprite;
            }

            if (_OpenEffectRoot != null)
            {
                _OpenEffectRoot.gameObject.SetActive(true);
            }

            Opened?.Invoke();
        }

        private void ApplyClosedVisual()
        {
            if (_ChestImage != null && _ClosedChestSprite != null)
            {
                _ChestImage.sprite = _ClosedChestSprite;
            }
        }
    }
}
