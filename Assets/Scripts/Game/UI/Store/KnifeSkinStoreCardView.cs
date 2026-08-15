using Game.Store.Data;
using Game.Store.Rewards;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Card view used for knife skin store items.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class KnifeSkinStoreCardView : StoreCardView
    {
        [Header("Knife Skin State UI")]
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
            PricePill?.SetVisible(true);
            SetCardInputGate(true);
            SetInteractable(canInteract);
        }

        public void SetStoreState(bool owned, bool canInteract)
        {
            EnsureAnimationState();
            _CanvasGroup.alpha = 1f;
            PricePill?.SetVisible(true);

            if (owned)
            {
                PricePill?.SetOwned();
                // Owned is a status, not a disabled visual state. Keep the button
                // presenter visually available, then block input at the card gate.
                SetInteractable(true);
                SetCardInputGate(false);
                return;
            }

            SetCardInputGate(true);
            SetInteractable(canInteract);
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
                _CanvasGroup.interactable = true;
                _CanvasGroup.blocksRaycasts = true;
            }

            if (_LayoutElement != null)
            {
                _LayoutElement.minWidth = _BaseMinWidth;
                _LayoutElement.preferredWidth = _BasePreferredWidth;
            }

            PricePill?.SetVisible(true);

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

        private void SetCardInputGate(bool enabled)
        {
            if (_CanvasGroup == null)
                return;

            _CanvasGroup.interactable = enabled;
            _CanvasGroup.blocksRaycasts = enabled;
        }
    }
}
