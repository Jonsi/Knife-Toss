using DG.Tweening;
using Game.Economy.Data;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using TMPro;
using UnityEngine;
using VContainer;

namespace Game.UI.Store
{
    /// <summary>
    /// Top-right currency balance chip.
    /// </summary>
    public class StoreBalanceChipView : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text BalanceText { get; private set; }
        [field: SerializeField] public CurrencyType Currency { get; private set; }
        [field: SerializeField] public float CountTweenDuration { get; private set; } = 0.35f;
        [field: SerializeField] public float ScalePulseMultiplier { get; private set; } = 1.12f;
        [field: SerializeField] public float ScalePulseDuration { get; private set; } = 0.18f;
        [field: SerializeField] public Color IncreaseFlashColor { get; private set; } = new Color(1f, 0.9f, 0.35f, 1f);
        [field: SerializeField] public Color DecreaseFlashColor { get; private set; } = new Color(1f, 0.45f, 0.35f, 1f);
        [field: SerializeField] public float ColorFlashDuration { get; private set; } = 0.2f;

        private ICurrencyService _CurrencyService;
        private CurrencyBalanceChangedEventChannel _BalanceChangedChannel;
        private string _ResolvedCurrencyId;
        private bool _IsSubscribed;
        private Sequence _ActiveSequence;
        private Tween _ValueTween;
        private int _DisplayedValue;
        private Color _BaseTextColor = Color.white;
        private Vector3 _BaseScale = Vector3.one;
        private bool _HasVisualState;

        [Inject]
        private void Construct(
            ICurrencyService currencyService,
            CurrencyTypeCatalog currencyTypeCatalog,
            CurrencyBalanceChangedEventChannel balanceChangedChannel)
        {
            _CurrencyService = currencyService;
            _BalanceChangedChannel = balanceChangedChannel;

            if (Currency == CurrencyType.None)
            {
                var currencies = currencyTypeCatalog?.GetAllConfigs();
                if (currencies != null && currencies.Count > 0 && currencies[0] != null)
                {
                    Currency = CurrencyTypeExtensions.FromTypeId(currencies[0].TypeId);
                }
            }

            _ResolvedCurrencyId = Currency.ToTypeId();

            if (isActiveAndEnabled)
            {
                Subscribe();
                Refresh();
            }
        }

        private void Awake()
        {
            CacheVisualState();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
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

        public void SetBalance(int value)
        {
            CacheVisualState();
            KillActiveTweens(resetVisuals: true);
            _DisplayedValue = value;

            if (BalanceText != null)
            {
                BalanceText.text = _DisplayedValue.ToString();
            }
        }

        private void Refresh()
        {
            if (_CurrencyService == null || string.IsNullOrEmpty(_ResolvedCurrencyId))
            {
                return;
            }

            SetBalance(_CurrencyService.GetBalance(_ResolvedCurrencyId));
        }

        private void OnBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            if (!string.IsNullOrEmpty(_ResolvedCurrencyId) && data.CurrencyId == _ResolvedCurrencyId)
            {
                AnimateBalance(data.NewBalance, data.AmountChanged);
            }
        }

        private void Subscribe()
        {
            if (_IsSubscribed || _BalanceChangedChannel == null)
            {
                return;
            }

            _BalanceChangedChannel.Subscribe(OnBalanceChanged);
            _IsSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_IsSubscribed || _BalanceChangedChannel == null)
            {
                return;
            }

            _BalanceChangedChannel.Unsubscribe(OnBalanceChanged);
            _IsSubscribed = false;
        }

        private void AnimateBalance(int value, int amountChanged)
        {
            CacheVisualState();
            KillActiveTweens(resetVisuals: true);

            if (BalanceText == null)
            {
                _DisplayedValue = value;
                return;
            }

            if (amountChanged == 0 || _DisplayedValue == value)
            {
                SetBalance(value);
                return;
            }

            _ActiveSequence = DOTween.Sequence();
            _ValueTween = DOTween.To(
                () => _DisplayedValue,
                currentValue =>
                {
                    _DisplayedValue = currentValue;
                    BalanceText.text = currentValue.ToString();
                },
                value,
                CountTweenDuration).SetEase(Ease.OutQuad);

            _ActiveSequence.Join(_ValueTween);
            _ActiveSequence.Join(BalanceText.rectTransform.DOScale(_BaseScale * ScalePulseMultiplier, ScalePulseDuration).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad));

            Color flashColor = amountChanged > 0 ? IncreaseFlashColor : DecreaseFlashColor;
            _ActiveSequence.Join(BalanceText.DOColor(flashColor, ColorFlashDuration));
            _ActiveSequence.Append(BalanceText.DOColor(_BaseTextColor, ColorFlashDuration));
            _ActiveSequence.OnComplete(() =>
            {
                _DisplayedValue = value;
                BalanceText.text = value.ToString();
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
    }
}
