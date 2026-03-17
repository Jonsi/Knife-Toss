using DG.Tweening;
using Common.Events;
using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Entities.Collectible.Data;
using Game.Flow.Events;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.UI.HUD
{
    /// <summary>
    /// Displays the player's coin balance (first currency in the catalog).
    /// </summary>
    public class CoinsUiDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _CountText;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private CurrencyBalanceChangedEventChannel _BalanceChangedChannel;
        [SerializeField] private float _CountTweenDuration = 0.35f;
        [SerializeField] private float _ScalePulseMultiplier = 1.12f;
        [SerializeField] private float _ScalePulseDuration = 0.18f;
        [SerializeField] private Color _IncreaseFlashColor = new Color(1f, 0.9f, 0.35f, 1f);
        [SerializeField] private Color _DecreaseFlashColor = new Color(1f, 0.45f, 0.35f, 1f);
        [SerializeField] private float _ColorFlashDuration = 0.2f;

        private ICurrencyService _CurrencyService;
        private IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private CurrencyAccumulationConfig _AccumulationConfig;
        private string _CoinCurrencyId;
        private Sequence _ActiveSequence;
        private Tween _ValueTween;
        private int _DisplayedValue;
        private Color _BaseTextColor = Color.white;
        private Vector3 _BaseScale = Vector3.one;
        private bool _HasVisualState;
        private Canvas _ParentCanvas;
        private CurrencyBurstAnimator _BurstAnimator;
        private bool _IsCollectibleHitSubscribed;

        [Inject]
        private void Construct(
            ICurrencyService currencyService,
            CurrencyTypeCatalog catalog,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            CurrencyAccumulationConfig accumulationConfig)
        {
            _CurrencyService = currencyService;
            _CollectibleHitChannel = collectibleHitChannel;
            _AccumulationConfig = accumulationConfig;
            var configs = catalog?.GetAllConfigs();
            _CoinCurrencyId = (configs != null && configs.Count > 0 && configs[0] != null)
                ? (configs[0].TypeId ?? string.Empty)
                : string.Empty;

            Debug.Log($"[CoinsUiDisplay] Constructed. CurrencyId={_CoinCurrencyId}, HasCollectibleChannel={_CollectibleHitChannel != null}, HasAccumulationConfig={_AccumulationConfig != null}, HasAccumulationIcon={_AccumulationConfig?.IconSprite != null}", this);

            if (isActiveAndEnabled)
            {
                SubscribeToCollectibleHits();
            }
        }

        private void Awake()
        {
            CacheVisualState();
            _ParentCanvas = GetComponentInParent<Canvas>();
            Debug.Log($"[CoinsUiDisplay] Awake. HasCanvas={_ParentCanvas != null}, HasText={_CountText != null}", this);
        }

        private void OnEnable()
        {
            Debug.Log("[CoinsUiDisplay] OnEnable subscribing to channels.", this);
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            if (_BalanceChangedChannel != null)
                _BalanceChangedChannel.Subscribe(OnBalanceChanged);
            SubscribeToCollectibleHits();

            SyncFromCurrency();
        }

        private void OnDisable()
        {
            Debug.Log("[CoinsUiDisplay] OnDisable unsubscribing from channels.", this);
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_BalanceChangedChannel != null)
                _BalanceChangedChannel.Unsubscribe(OnBalanceChanged);
            UnsubscribeFromCollectibleHits();

            KillActiveTweens(resetVisuals: true);
        }

        private void OnDestroy()
        {
            KillActiveTweens(resetVisuals: false);
        }

        private void OnLevelStarted(LevelStartedEventData _)
        {
            SyncFromCurrency();
        }

        private void OnBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            if (!string.IsNullOrEmpty(_CoinCurrencyId) && data.CurrencyId == _CoinCurrencyId)
                AnimateToBalance(data.NewBalance, data.AmountChanged);
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            if (data.Value <= 0 || _CountText == null)
            {
                Debug.Log($"[CoinsUiDisplay] Skipping collectible accumulation. Value={data.Value}, HasText={_CountText != null}", this);
                return;
            }

            Debug.Log($"[CoinsUiDisplay] Collectible hit received. WorldPosition={data.Position}, Value={data.Value}", this);
            PlayAccumulationFromWorld(data.Position, data.Value);
        }

        private void SyncFromCurrency()
        {
            if (_CurrencyService == null || string.IsNullOrEmpty(_CoinCurrencyId))
                return;

            SetBalanceImmediate(_CurrencyService.GetBalance(_CoinCurrencyId));
        }

        private void SetBalanceImmediate(int value)
        {
            CacheVisualState();
            KillActiveTweens(resetVisuals: true);
            _DisplayedValue = value;

            if (_CountText != null)
                _CountText.text = _DisplayedValue.ToString();
        }

        private void AnimateToBalance(int value, int amountChanged)
        {
            CacheVisualState();
            KillActiveTweens(resetVisuals: true);

            if (_CountText == null)
            {
                _DisplayedValue = value;
                return;
            }

            if (amountChanged == 0 || _DisplayedValue == value)
            {
                SetBalanceImmediate(value);
                return;
            }

            _ActiveSequence = DOTween.Sequence();
            _ValueTween = DOTween.To(
                () => _DisplayedValue,
                currentValue =>
                {
                    _DisplayedValue = currentValue;
                    _CountText.text = currentValue.ToString();
                },
                value,
                _CountTweenDuration).SetEase(Ease.OutQuad);

            _ActiveSequence.Join(_ValueTween);
            _ActiveSequence.Join(_CountText.rectTransform.DOScale(_BaseScale * _ScalePulseMultiplier, _ScalePulseDuration).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad));

            Color flashColor = amountChanged > 0 ? _IncreaseFlashColor : _DecreaseFlashColor;
            _ActiveSequence.Join(_CountText.DOColor(flashColor, _ColorFlashDuration));
            _ActiveSequence.Append(_CountText.DOColor(_BaseTextColor, _ColorFlashDuration));
            _ActiveSequence.OnComplete(() =>
            {
                _DisplayedValue = value;
                _CountText.text = value.ToString();
                _CountText.color = _BaseTextColor;
                _CountText.rectTransform.localScale = _BaseScale;
                _ActiveSequence = null;
                _ValueTween = null;
            });
        }

        private void CacheVisualState()
        {
            if (_HasVisualState || _CountText == null)
            {
                return;
            }

            _BaseTextColor = _CountText.color;
            _BaseScale = _CountText.rectTransform.localScale;
            _HasVisualState = true;
        }

        private void PlayAccumulationFromWorld(Vector2 worldPosition, int amount)
        {
            if (!EnsureBurstAnimator())
            {
                Debug.LogWarning($"[CoinsUiDisplay] Burst animator unavailable. HasCanvas={_ParentCanvas != null}, HasText={_CountText != null}, HasConfig={_AccumulationConfig != null}, HasIcon={_AccumulationConfig?.IconSprite != null}", this);
                return;
            }

            Camera worldCamera = Camera.main;
            if (worldCamera == null)
            {
                Debug.LogWarning("[CoinsUiDisplay] No main camera found for world-to-screen conversion.", this);
                return;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
            Debug.Log($"[CoinsUiDisplay] Playing accumulation from screen point {screenPoint} toward {_CountText.rectTransform.position}.", this);
            _BurstAnimator.PlayFromScreenPoint(screenPoint, amount);
        }

        private bool EnsureBurstAnimator()
        {
            if (_BurstAnimator != null && _BurstAnimator.IsValid)
            {
                return true;
            }

            if (_ParentCanvas == null)
            {
                _ParentCanvas = GetComponentInParent<Canvas>();
            }

            if (_ParentCanvas == null || _CountText == null || _AccumulationConfig == null)
            {
                return false;
            }

            _BurstAnimator = new CurrencyBurstAnimator(_ParentCanvas, _CountText.rectTransform, _AccumulationConfig);
            return _BurstAnimator.IsValid;
        }

        private void SubscribeToCollectibleHits()
        {
            if (_IsCollectibleHitSubscribed || _CollectibleHitChannel == null)
            {
                return;
            }

            _CollectibleHitChannel.Subscribe(OnCollectibleHit);
            _IsCollectibleHitSubscribed = true;
            Debug.Log("[CoinsUiDisplay] Subscribed to collectible hit channel.", this);
        }

        private void UnsubscribeFromCollectibleHits()
        {
            if (!_IsCollectibleHitSubscribed || _CollectibleHitChannel == null)
            {
                return;
            }

            _CollectibleHitChannel.Unsubscribe(OnCollectibleHit);
            _IsCollectibleHitSubscribed = false;
            Debug.Log("[CoinsUiDisplay] Unsubscribed from collectible hit channel.", this);
        }

        private void KillActiveTweens(bool resetVisuals)
        {
            _ValueTween?.Kill();
            _ValueTween = null;

            _ActiveSequence?.Kill();
            _ActiveSequence = null;

            if (resetVisuals && _CountText != null && _HasVisualState)
            {
                _CountText.color = _BaseTextColor;
                _CountText.rectTransform.localScale = _BaseScale;
            }
        }
    }
}
