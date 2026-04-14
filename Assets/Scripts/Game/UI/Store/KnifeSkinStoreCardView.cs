using Game.Store.Data;
using Game.Store.Rewards;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private Button _EquipButton;
        [SerializeField] private float _PurchasedOutDuration = 0.26f;
        [SerializeField] private float _PurchasedCollapseDuration = 0.18f;
        [SerializeField] private float _PurchasedOutOffsetY = 48f;

        private CanvasGroup _CanvasGroup;
        private RectTransform _RectTransform;
        private LayoutElement _LayoutElement;
        private Vector3 _BaseScale;
        private Vector2 _BaseSizeDelta;
        private float _BaseMinWidth;
        private float _BasePreferredWidth;
        private bool _HasCachedTransformState;
        private bool _IsPurchasedOut;

        private void Awake()
        {
            if (_EquipButton != null)
            {
                _EquipButton.gameObject.SetActive(false);
            }
        }

        public void SetAvailableState(bool canInteract)
        {
            if (_IsPurchasedOut)
            {
                return;
            }

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

        public async UniTask PlayPurchasedOutAsync()
        {
            if (_IsPurchasedOut)
            {
                return;
            }

            _IsPurchasedOut = true;
            EnsureAnimationState();
            SetInteractable(false);
            SetPricePillVisible(false);

            var startPosition = _RectTransform.anchoredPosition;
            var targetPosition = startPosition + Vector2.up * _PurchasedOutOffsetY;
            var sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);
            sequence.Join(_RectTransform.DOAnchorPos(targetPosition, _PurchasedOutDuration).SetEase(Ease.InQuad));
            sequence.Join(_CanvasGroup.DOFade(0f, _PurchasedOutDuration).SetEase(Ease.InQuad));

            await UniTask.Delay(Mathf.CeilToInt(_PurchasedOutDuration * 1000f), ignoreTimeScale: true);

            if (sequence.IsActive())
            {
                sequence.Kill();
            }

            if (_LayoutElement != null)
            {
                var collapsedSize = _BaseSizeDelta;
                collapsedSize.x = 0f;
                var collapseSequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);
                collapseSequence.Join(_RectTransform.DOSizeDelta(collapsedSize, _PurchasedCollapseDuration).SetEase(Ease.InQuad));
                collapseSequence.Join(DOTween.To(
                    () => _LayoutElement.preferredWidth,
                    width => _LayoutElement.preferredWidth = width,
                    0f,
                    _PurchasedCollapseDuration).SetEase(Ease.InQuad));
                collapseSequence.Join(DOTween.To(
                    () => _LayoutElement.minWidth,
                    width => _LayoutElement.minWidth = width,
                    0f,
                    _PurchasedCollapseDuration).SetEase(Ease.InQuad));

                await UniTask.Delay(Mathf.CeilToInt(_PurchasedCollapseDuration * 1000f), ignoreTimeScale: true);

                if (collapseSequence.IsActive())
                {
                    collapseSequence.Kill();
                }
            }

            Destroy(gameObject);
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
