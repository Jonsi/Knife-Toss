using Common.Events;
using Cysharp.Threading.Tasks;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using Game.UI.Store;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Popups
{
    /// <summary>
    /// Popup shown when the player fails a level.
    /// Exposes three outcomes: Retry (same level), Restart Stage, and Continue (watch a rewarded ad).
    /// The popup itself is ad-agnostic — it raises event channels and lets controllers decide what happens.
    /// Call <see cref="SetContinueAvailable"/> to show/hide the Continue button based on ad availability.
    /// </summary>
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;
        [SerializeField] private Button _ContinueButton;
        [SerializeField] private Transform _PowerUpsRoot;

        [Header("Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;
        [SerializeField] private VoidEventChannel _ContinueRequestedChannel;

        private PowerUpStoreCardView[] _PowerUpCards;
        private StoreCatalog _StoreCatalog;
        private IPurchaseService _PurchaseService;
        private PurchaseStartedEventChannel _PurchaseStartedChannel;
        private PurchaseCompletedEventChannel _PurchaseCompletedChannel;
        private PurchaseFailedEventChannel _PurchaseFailedChannel;

        [Inject]
        private void Construct(
            StoreCatalog storeCatalog,
            IPurchaseService purchaseService,
            PurchaseStartedEventChannel purchaseStartedChannel,
            PurchaseCompletedEventChannel purchaseCompletedChannel,
            PurchaseFailedEventChannel purchaseFailedChannel)
        {
            _StoreCatalog = storeCatalog;
            _PurchaseService = purchaseService;
            _PurchaseStartedChannel = purchaseStartedChannel;
            _PurchaseCompletedChannel = purchaseCompletedChannel;
            _PurchaseFailedChannel = purchaseFailedChannel;
        }

        private void Start()
        {
            CachePowerUpCards();
            BindPowerUpCards();
            RefreshPowerUpCards();
        }

        private void OnEnable()
        {
            _RetryButton.onClick.AddListener(OnRetryClicked);
            _RestartStageButton.onClick.AddListener(OnRestartStageClicked);

            if (_ContinueButton != null)
                _ContinueButton.onClick.AddListener(OnContinueClicked);

            _PurchaseStartedChannel?.Subscribe(OnPurchaseStateChanged);
            _PurchaseCompletedChannel?.Subscribe(OnPurchaseStateChanged);
            _PurchaseFailedChannel?.Subscribe(OnPurchaseStateChanged);
        }

        private void OnDisable()
        {
            _RetryButton.onClick.RemoveListener(OnRetryClicked);
            _RestartStageButton.onClick.RemoveListener(OnRestartStageClicked);

            if (_ContinueButton != null)
                _ContinueButton.onClick.RemoveListener(OnContinueClicked);

            _PurchaseStartedChannel?.Unsubscribe(OnPurchaseStateChanged);
            _PurchaseCompletedChannel?.Unsubscribe(OnPurchaseStateChanged);
            _PurchaseFailedChannel?.Unsubscribe(OnPurchaseStateChanged);
        }

        /// <summary>
        /// Shows or hides the Continue button based on whether a rewarded ad is available.
        /// Called by <see cref="Game.Ads.Controllers.AdRewardController"/> each time the popup appears.
        /// </summary>
        public void SetContinueAvailable(bool available)
        {
            if (_ContinueButton != null)
                _ContinueButton.gameObject.SetActive(available);
        }

        public override void Show()
        {
            RefreshPowerUpCards();
            base.Show();
        }

        private void OnRetryClicked()
        {
            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Raise();
        }

        private void OnRestartStageClicked()
        {
            if (_RestartStageChannel != null)
                _RestartStageChannel.Raise();
        }

        private void OnContinueClicked()
        {
            if (_ContinueRequestedChannel != null)
                _ContinueRequestedChannel.Raise();
        }

        private void CachePowerUpCards()
        {
            if (_PowerUpsRoot == null)
            {
                _PowerUpCards = System.Array.Empty<PowerUpStoreCardView>();
                return;
            }

            _PowerUpCards = _PowerUpsRoot.GetComponentsInChildren<PowerUpStoreCardView>(true);
        }

        private void BindPowerUpCards()
        {
            if (_PowerUpCards == null || _StoreCatalog?.PowerUps == null)
            {
                SetPowerUpsVisible(false);
                return;
            }

            var visibleCount = Mathf.Min(_PowerUpCards.Length, _StoreCatalog.PowerUps.Count);
            for (var i = 0; i < _PowerUpCards.Length; i++)
            {
                var card = _PowerUpCards[i];
                var isVisible = card != null && i < visibleCount;
                if (card == null)
                {
                    continue;
                }

                card.gameObject.SetActive(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                card.Setup(_StoreCatalog.PowerUps[i], OnPowerUpBuyClicked);
            }

            SetPowerUpsVisible(visibleCount > 0);
        }

        private void RefreshPowerUpCards()
        {
            if (_PowerUpCards == null || _PowerUpCards.Length == 0 || _StoreCatalog?.PowerUps == null || _PurchaseService == null)
            {
                SetPowerUpsVisible(false);
                return;
            }

            SetPowerUpsVisible(true);

            for (var i = 0; i < _PowerUpCards.Length && i < _StoreCatalog.PowerUps.Count; i++)
            {
                var card = _PowerUpCards[i];
                if (card == null || !card.gameObject.activeSelf)
                {
                    continue;
                }

                var entry = _StoreCatalog.PowerUps[i];
                card.SetInteractable(!_PurchaseService.IsPurchaseInProgress && _PurchaseService.CanPurchase(entry));
            }
        }

        private void SetPowerUpsVisible(bool visible)
        {
            if (_PowerUpsRoot != null)
            {
                _PowerUpsRoot.gameObject.SetActive(visible);
            }
        }

        private void OnPowerUpBuyClicked(PurchasableEntry entry)
        {
            TryBuyPowerUpAsync(entry).Forget();
        }

        private async UniTaskVoid TryBuyPowerUpAsync(PurchasableEntry entry)
        {
            if (_PurchaseService == null || entry == null)
            {
                return;
            }

            await _PurchaseService.TryPurchaseAsync(entry);
            RefreshPowerUpCards();
        }

        private void OnPurchaseStateChanged<T>(T _)
        {
            RefreshPowerUpCards();
        }
    }
}
