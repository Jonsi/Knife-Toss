using DG.Tweening;
using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Flow.Events;
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
        private string _CoinCurrencyId;
        private Sequence _ActiveSequence;
        private Tween _ValueTween;
        private int _DisplayedValue;
        private Color _BaseTextColor = Color.white;
        private Vector3 _BaseScale = Vector3.one;
        private bool _HasVisualState;

        [Inject]
        private void Construct(ICurrencyService currencyService, CurrencyTypeCatalog catalog)
        {
            _CurrencyService = currencyService;
            var configs = catalog?.GetAllConfigs();
            _CoinCurrencyId = (configs != null && configs.Count > 0 && configs[0] != null)
                ? (configs[0].TypeId ?? string.Empty)
                : string.Empty;
        }

        private void Awake()
        {
            CacheVisualState();
        }

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            if (_BalanceChangedChannel != null)
                _BalanceChangedChannel.Subscribe(OnBalanceChanged);

            SyncFromCurrency();
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_BalanceChangedChannel != null)
                _BalanceChangedChannel.Unsubscribe(OnBalanceChanged);

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
