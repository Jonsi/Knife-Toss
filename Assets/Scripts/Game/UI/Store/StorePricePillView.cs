using Game.Economy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Shared, prefab-owned Store price presentation. Currency and amount remain Unity UI content.
    /// </summary>
    public sealed class StorePricePillView : MonoBehaviour
    {
        private const float FullWidthHorizontalPadding = 40f;
        private const float OwnedMinimumFontSize = 24f;
        private const float OwnedMaximumFontSize = 28f;
        private const float OwnedCharacterSpacing = 0f;
        private const float OwnedIconSize = 42f;
        private static readonly Color OwnedTextColor = new Color32(245, 226, 183, 255);

        [Header("Modular Layers")]
        [SerializeField] private Image _FillImage;
        [SerializeField] private Image _BorderImage;
        [SerializeField] private Image _IconImage;
        [SerializeField] private Sprite _CurrencyIconSprite;
        [SerializeField] private Sprite _OwnedIconSprite;
        [SerializeField] private TMP_Text _AmountText;
        [SerializeField] private Color _PriceFillColor = new Color32(150, 78, 28, 255);
        [SerializeField] private Color _OwnedFillColor = new Color32(48, 140, 64, 255);

        private RectTransform _AmountRectTransform;
        private float _DefaultAmountWidth;
        private float _DefaultMinimumFontSize;
        private float _DefaultMaximumFontSize;
        private float _DefaultCharacterSpacing;
        private Color _DefaultTextColor;
        private Vector2 _DefaultIconSize;
        private bool _HasCachedLayout;

        public Image FillImage => _FillImage;
        public Image BorderImage => _BorderImage;
        public Image IconImage => _IconImage;
        public Sprite CurrencyIconSprite => _CurrencyIconSprite;
        public Sprite OwnedIconSprite => _OwnedIconSprite;
        public TMP_Text AmountText => _AmountText;

        public void SetPrice(int amount, CurrencyType currencyType)
        {
            SetFillState(false);
            SetIconState(currencyType != CurrencyType.None, false);
            SetAmountTextLayout(false, false);

            if (_AmountText != null)
            {
                _AmountText.text = amount.ToString();
            }
        }

        public void SetCustomPrice(string text)
        {
            SetFillState(false);
            SetIconState(false, false);
            SetAmountTextLayout(true, false);

            if (_AmountText != null)
            {
                _AmountText.text = text ?? string.Empty;
            }
        }

        public void SetOwned()
        {
            SetFillState(true);
            SetIconState(true, true);
            SetAmountTextLayout(false, true);

            if (_AmountText != null)
            {
                _AmountText.text = "OWNED";
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void SetIconState(bool visible, bool owned)
        {
            EnsureLayoutDefaults();
            if (_IconImage == null)
            {
                return;
            }

            _IconImage.gameObject.SetActive(visible);
            _IconImage.sprite = owned ? _OwnedIconSprite : _CurrencyIconSprite;
            _IconImage.color = Color.white;
            _IconImage.preserveAspect = true;
            _IconImage.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                owned ? OwnedIconSize : _DefaultIconSize.x);
            _IconImage.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                owned ? OwnedIconSize : _DefaultIconSize.y);

        }

        private void SetFillState(bool owned)
        {
            if (_FillImage != null)
            {
                _FillImage.color = owned
                    ? _OwnedFillColor
                    : _PriceFillColor;
            }
        }

        private void SetAmountTextLayout(bool fullWidth, bool owned)
        {
            EnsureLayoutDefaults();
            if (_AmountText == null || _AmountRectTransform == null)
            {
                return;
            }

            _AmountText.textWrappingMode = TextWrappingModes.NoWrap;
            _AmountText.fontSizeMin = owned
                ? OwnedMinimumFontSize
                : _DefaultMinimumFontSize;
            _AmountText.fontSizeMax = owned
                ? Mathf.Min(_DefaultMaximumFontSize, OwnedMaximumFontSize)
                : _DefaultMaximumFontSize;
            _AmountText.characterSpacing = owned
                ? OwnedCharacterSpacing
                : _DefaultCharacterSpacing;
            _AmountText.color = owned
                ? OwnedTextColor
                : _DefaultTextColor;
            var width = _DefaultAmountWidth;
            if (fullWidth && transform is RectTransform rootRectTransform)
            {
                width = Mathf.Max(
                    _DefaultAmountWidth,
                    rootRectTransform.rect.width - FullWidthHorizontalPadding);
            }

            _AmountRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private void EnsureLayoutDefaults()
        {
            if (_HasCachedLayout)
            {
                return;
            }

            _AmountRectTransform = _AmountText != null
                ? _AmountText.rectTransform
                : null;
            _DefaultAmountWidth = _AmountRectTransform != null
                ? _AmountRectTransform.rect.width
                : 0f;
            _DefaultMinimumFontSize = _AmountText != null
                ? _AmountText.fontSizeMin
                : 0f;
            _DefaultMaximumFontSize = _AmountText != null
                ? _AmountText.fontSizeMax
                : 0f;
            _DefaultCharacterSpacing = _AmountText != null
                ? _AmountText.characterSpacing
                : 0f;
            _DefaultTextColor = _AmountText != null
                ? _AmountText.color
                : Color.white;
            _DefaultIconSize = _IconImage != null
                ? _IconImage.rectTransform.sizeDelta
                : Vector2.zero;
            _HasCachedLayout = true;
        }
    }
}
