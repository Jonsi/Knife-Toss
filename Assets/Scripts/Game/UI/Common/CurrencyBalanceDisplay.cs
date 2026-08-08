using DG.Tweening;
using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Feedback.Currency;
using TMPro;
using UnityEngine;

namespace Game.UI.Common
{
    /// <summary>
    /// Shared currency balance presenter used by scene-specific wrapper views.
    /// </summary>
    public class CurrencyBalanceDisplay : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text BalanceText { get; private set; }
        [field: SerializeField] public CurrencyType Currency { get; private set; }
        [field: SerializeField] public CurrencyBalanceChangedEventChannel BalanceChangedChannel { get; private set; }
        [field: SerializeField] public float CountTweenDuration { get; private set; } = 0.35f;
        [field: SerializeField] public float ScalePulseMultiplier { get; private set; } = 1.12f;
        [field: SerializeField] public float ScalePulseDuration { get; private set; } = 0.18f;
        [field: SerializeField] public Color IncreaseFlashColor { get; private set; } = new Color(1f, 0.9f, 0.35f, 1f);
        [field: SerializeField] public Color DecreaseFlashColor { get; private set; } = new Color(1f, 0.45f, 0.35f, 1f);
        [field: SerializeField] public float ColorFlashDuration { get; private set; } = 0.2f;
        [field: SerializeField] public bool UseUnscaledTime { get; private set; } = true;

        private ICurrencyService _CurrencyService;
        private CurrencyAccumulationConfig _AccumulationConfig;
        private string _ResolvedCurrencyId;
        private bool _IsSubscribed;
        private Sequence _ActiveSequence;
        private Tween _ValueTween;
        private int _DisplayedValue;
        private Color _BaseTextColor = Color.white;
        private Vector3 _BaseScale = Vector3.one;
        private bool _HasVisualState;
        private Canvas _ParentCanvas;
        private CurrencyBurstAnimator _BurstAnimator;
        private bool _IsInitialized;

        public void Initialize(
            ICurrencyService currencyService,
            CurrencyTypeCatalog currencyTypeCatalog,
            CurrencyAccumulationConfig accumulationConfig)
        {
            _CurrencyService = currencyService;
            _AccumulationConfig = accumulationConfig;

            if (Currency == CurrencyType.None)
            {
                var currencies = currencyTypeCatalog?.GetAllConfigs();
                if (currencies != null && currencies.Count > 0 && currencies[0] != null)
                {
                    Currency = CurrencyTypeExtensions.FromTypeId(currencies[0].TypeId);
                }
            }

            _ResolvedCurrencyId = Currency.ToTypeId();
            _IsInitialized = true;

            if (isActiveAndEnabled)
            {
                Subscribe();
                RefreshImmediate();
            }
        }

        private void Awake()
        {
            CacheVisualState();
            _ParentCanvas = GetComponentInParent<Canvas>();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshImmediate();
        }

        private void OnDisable()
        {
            Unsubscribe();
            KillActiveTweens(resetVisuals: true);
        }

        private void OnDestroy()
        {
            KillActiveTweens(resetVisuals: false);
        }

        public void RefreshImmediate()
        {
            if (!_IsInitialized || _CurrencyService == null || string.IsNullOrEmpty(_ResolvedCurrencyId))
            {
                return;
            }

            SetBalanceImmediate(_CurrencyService.GetBalance(_ResolvedCurrencyId));
        }

        public void PlayAccumulationFromWorld(Vector2 worldPosition, int amount)
        {
            if (amount <= 0 || BalanceText == null || !EnsureBurstAnimator())
            {
                return;
            }

            Camera worldCamera = Camera.main;
            if (worldCamera == null)
            {
                return;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
            _BurstAnimator.PlayFromScreenPoint(screenPoint, amount);
        }

        public void PlayAccumulationFromUiElement(RectTransform source, int amount)
        {
            if (source == null || amount <= 0 || !EnsureBurstAnimator())
            {
                return;
            }

            Camera uiCamera = _ParentCanvas != null && _ParentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _ParentCanvas.worldCamera
                : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, source.position);
            _BurstAnimator.PlayFromScreenPoint(screenPoint, amount);
        }

        private void SetBalanceImmediate(int value)
        {
            CacheVisualState();
            KillActiveTweens(resetVisuals: true);
            _DisplayedValue = value;

            if (BalanceText != null)
            {
                BalanceText.text = FormatBalance(_DisplayedValue);
            }
        }

        private void OnBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            if (!string.IsNullOrEmpty(_ResolvedCurrencyId) && data.CurrencyId == _ResolvedCurrencyId)
            {
                AnimateBalance(data.NewBalance, CurrencyFeedbackSemantics.Classify(data));
            }
        }

        private void Subscribe()
        {
            if (!_IsInitialized || _IsSubscribed || BalanceChangedChannel == null)
            {
                return;
            }

            BalanceChangedChannel.Subscribe(OnBalanceChanged);
            _IsSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_IsSubscribed || BalanceChangedChannel == null)
            {
                return;
            }

            BalanceChangedChannel.Unsubscribe(OnBalanceChanged);
            _IsSubscribed = false;
        }

        private void AnimateBalance(int value, CurrencyFeedbackKind feedbackKind)
        {
            CacheVisualState();
            KillActiveTweens(resetVisuals: true);

            if (BalanceText == null)
            {
                _DisplayedValue = value;
                return;
            }

            if (feedbackKind == CurrencyFeedbackKind.None || _DisplayedValue == value)
            {
                SetBalanceImmediate(value);
                return;
            }

            _ActiveSequence = DOTween.Sequence();
            _ActiveSequence.SetUpdate(UseUnscaledTime);
            _ValueTween = DOTween.To(
                () => _DisplayedValue,
                currentValue =>
                {
                    _DisplayedValue = currentValue;
                    BalanceText.text = FormatBalance(currentValue);
                },
                value,
                CountTweenDuration).SetEase(Ease.OutQuad).SetUpdate(UseUnscaledTime);

            _ActiveSequence.Join(_ValueTween);
            _ActiveSequence.Join(BalanceText.rectTransform.DOScale(_BaseScale * ScalePulseMultiplier, ScalePulseDuration).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad).SetUpdate(UseUnscaledTime));

            Color flashColor = feedbackKind == CurrencyFeedbackKind.Gain
                ? IncreaseFlashColor
                : DecreaseFlashColor;
            _ActiveSequence.Join(BalanceText.DOColor(flashColor, ColorFlashDuration).SetUpdate(UseUnscaledTime));
            _ActiveSequence.Append(BalanceText.DOColor(_BaseTextColor, ColorFlashDuration).SetUpdate(UseUnscaledTime));
            _ActiveSequence.OnComplete(() =>
            {
                _DisplayedValue = value;
                BalanceText.text = FormatBalance(value);
                BalanceText.color = _BaseTextColor;
                BalanceText.rectTransform.localScale = _BaseScale;
                _ActiveSequence = null;
                _ValueTween = null;
            });
        }

        private void CacheVisualState()
        {
            if (_HasVisualState || BalanceText == null)
            {
                return;
            }

            _BaseTextColor = BalanceText.color;
            _BaseScale = BalanceText.rectTransform.localScale;
            _HasVisualState = true;
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

            if (_ParentCanvas == null || BalanceText == null || _AccumulationConfig == null)
            {
                return false;
            }

            _BurstAnimator = new CurrencyBurstAnimator(_ParentCanvas, BalanceText.rectTransform, _AccumulationConfig, UseUnscaledTime);
            return _BurstAnimator.IsValid;
        }

        private void KillActiveTweens(bool resetVisuals)
        {
            _ValueTween?.Kill();
            _ValueTween = null;

            _ActiveSequence?.Kill();
            _ActiveSequence = null;

            if (resetVisuals && BalanceText != null && _HasVisualState)
            {
                BalanceText.color = _BaseTextColor;
                BalanceText.rectTransform.localScale = _BaseScale;
            }
        }

        private static string FormatBalance(int value)
        {
            return value.ToString("N0");
        }
    }
}
