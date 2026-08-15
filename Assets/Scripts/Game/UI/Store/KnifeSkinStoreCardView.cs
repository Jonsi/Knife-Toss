using Game.Store.Data;
using Game.Store.Rewards;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Card view used for knife skin store items.
    /// </summary>
    public class KnifeSkinStoreCardView : StoreCardView
    {
        [Header("Knife Skin State UI")]
        [SerializeField] private GameObject _PricePillRoot;
        [SerializeField] private GameObject _StatusPillRoot;
        [SerializeField] private TMP_Text _StatusText;
        [SerializeField, Range(0.8f, 1f)] private float _SideCardScale = 0.9f;
        private CanvasGroup _CanvasGroup;
        private RectTransform _RectTransform;
        private LayoutElement _LayoutElement;
        private Vector3 _BaseScale;
        private Vector2 _BaseSizeDelta;
        private float _BaseMinWidth;
        private float _BasePreferredWidth;
        private bool _HasCachedTransformState;
        public void SetAvailableState(bool canInteract)
        {
            gameObject.SetActive(true);
            EnsureAnimationState();
            _RectTransform.localScale = _BaseScale;
            _RectTransform.sizeDelta = _BaseSizeDelta;
            _CanvasGroup.alpha = 1f;
            if (_LayoutElement != null)
            {
                _LayoutElement.minWidth = _BaseMinWidth;
                _LayoutElement.preferredWidth = _BasePreferredWidth;
            }
            SetPricePillVisible(true);
            SetInteractable(canInteract);
        }

        public void SetStoreState(bool owned, bool equipped, bool canInteract)
        {
            EnsureAnimationState();
            _CanvasGroup.alpha = 1f;
            SetPricePillVisible(!owned);

            if (_StatusPillRoot != null)
            {
                _StatusPillRoot.SetActive(owned);
            }

            if (_StatusText != null)
            {
                _StatusText.text = equipped ? "EQUIPPED" : "EQUIP";
            }

            SetInteractable(canInteract && !equipped);
        }

        public void SetCarouselSelected(bool selected)
        {
            transform.localScale = selected ? Vector3.one : Vector3.one * _SideCardScale;
        }

        public override void ResetForReuse()
        {
            DOTween.Kill(this);
            EnsureAnimationState();

            if (_RectTransform != null)
            {
                _RectTransform.localScale = _BaseScale;
                _RectTransform.sizeDelta = _BaseSizeDelta;
            }

            if (_CanvasGroup != null)
            {
                _CanvasGroup.alpha = 1f;
            }

            if (_LayoutElement != null)
            {
                _LayoutElement.minWidth = _BaseMinWidth;
                _LayoutElement.preferredWidth = _BasePreferredWidth;
            }

            SetPricePillVisible(true);
            if (_StatusPillRoot != null)
            {
                _StatusPillRoot.SetActive(false);
            }

            base.ResetForReuse();
        }

        protected override void BindEntry(PurchasableEntry entry)
        {
            if (entry?.Reward is KnifeSkinReward knifeSkinReward && knifeSkinReward.Config != null)
            {
                SetDisplaySprite(knifeSkinReward.Config.Icon);
            }
        }

        protected override void OnDisable()
        {
            DOTween.Kill(this);
            base.OnDisable();
        }

        private void SetPricePillVisible(bool visible)
        {
            if (_PricePillRoot != null)
            {
                _PricePillRoot.SetActive(visible);
            }

            PricePill?.SetVisible(visible);
        }

        private void EnsureAnimationState()
        {
            if (_RectTransform == null)
            {
                _RectTransform = transform as RectTransform;
            }

            if (_CanvasGroup == null)
            {
                _CanvasGroup = GetComponent<CanvasGroup>();
                if (_CanvasGroup == null)
                {
                    _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            _LayoutElement ??= GetComponent<LayoutElement>();

            if (_HasCachedTransformState || _RectTransform == null)
            {
                return;
            }

            _BaseScale = _RectTransform.localScale;
            _BaseSizeDelta = _RectTransform.sizeDelta;
            if (_LayoutElement != null)
            {
                _BaseMinWidth = _LayoutElement.minWidth;
                _BasePreferredWidth = _LayoutElement.preferredWidth;
            }
            _HasCachedTransformState = true;
        }
    }
}
