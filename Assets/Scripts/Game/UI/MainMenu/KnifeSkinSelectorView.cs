using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.MainMenu
{
    /// <summary>
    /// Scene-owned view for selecting the equipped knife skin on the main menu.
    /// </summary>
    public sealed class KnifeSkinSelectorView : MonoBehaviour
    {
        private const float FadeDuration = 0.22f;

        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private Image _PreviewImage;
        [SerializeField] private Button _PreviousButton;
        [SerializeField] private Button _NextButton;
        [SerializeField, Min(0f)] private float _EntranceDelay = 0.14f;
        [SerializeField] private float _PreviewFloatDistance = 14f;
        [SerializeField] private float _PreviewFloatDuration = 1.25f;
        [Header("Knife Swap")]
        [SerializeField, Min(0f)] private float _SwapDistance = 96f;
        [SerializeField, Min(0.01f)] private float _SwapOutDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float _SwapInDuration = 0.2f;
        [SerializeField, Range(0.5f, 1f)] private float _SwapScale = 0.9f;

        private RectTransform _RectTransform;
        private RectTransform _PreviewRectTransform;
        private Vector2 _BaseAnchoredPosition;
        private Vector2 _PreviewBaseAnchoredPosition;
        private Vector3 _BaseScale;
        private Vector3 _PreviewBaseScale;
        private Color _PreviewBaseColor = Color.white;
        private bool _HasCachedState;
        private bool _HasCachedPreviewState;
        private bool _CanCycle;
        private bool _CanInteract;
        private bool _IsSwapping;
        private Tween _PreviewFloatTween;
        private Sequence _SwapSequence;

        public bool IsSwapAnimating => _IsSwapping;

        private void Awake()
        {
            EnsureReferences();
            CacheState();
        }

        private void OnDisable()
        {
            StopSwap(resetVisual: true, restartFloat: false);
            StopPreviewFloat(resetPosition: true);
        }

        private void OnDestroy()
        {
            if (_SwapSequence != null && _SwapSequence.IsActive())
            {
                _SwapSequence.Kill();
            }

            DOTween.Kill(this);
        }

        public void Bind(Action onPrevious, Action onNext)
        {
            if (_PreviousButton != null)
            {
                _PreviousButton.onClick.RemoveAllListeners();
                if (onPrevious != null)
                {
                    _PreviousButton.onClick.AddListener(() => PlaySwap(onPrevious, -1));
                }
            }

            if (_NextButton != null)
            {
                _NextButton.onClick.RemoveAllListeners();
                if (onNext != null)
                {
                    _NextButton.onClick.AddListener(() => PlaySwap(onNext, 1));
                }
            }
        }

        public void SetDisplay(Sprite previewSprite, bool canCycle, bool canInteract)
        {
            EnsureReferences();
            CacheState();

            if (_PreviewImage != null)
            {
                _PreviewImage.sprite = previewSprite;
                _PreviewImage.enabled = previewSprite != null;
            }

            _CanCycle = canCycle;
            _CanInteract = canInteract;
            ApplyArrowAvailability();

            if (!_IsSwapping)
            {
                StartPreviewFloatIfNeeded();
            }
        }

        public void PlayIn()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            EnsureReferences();
            CacheState();
            StopSwap(resetVisual: true, restartFloat: false);
            DOTween.Kill(this);

            _RectTransform.anchoredPosition = _BaseAnchoredPosition + Vector2.down * 42f;
            _RectTransform.localScale = _BaseScale * 0.92f;
            _CanvasGroup.alpha = 0f;

            var sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);
            if (_EntranceDelay > 0f)
            {
                sequence.SetDelay(_EntranceDelay);
            }
            sequence.Join(_RectTransform.DOAnchorPos(_BaseAnchoredPosition, FadeDuration).SetEase(Ease.OutBack));
            sequence.Join(_RectTransform.DOScale(_BaseScale, FadeDuration).SetEase(Ease.OutBack));
            sequence.Join(_CanvasGroup.DOFade(1f, FadeDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(StartPreviewFloatIfNeeded);
        }

        public async UniTask PlayOutAsync()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            EnsureReferences();
            CacheState();
            StopSwap(resetVisual: true, restartFloat: false);
            DOTween.Kill(this);
            StopPreviewFloat(resetPosition: true);

            var sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);
            sequence.Join(_RectTransform.DOAnchorPos(_BaseAnchoredPosition + Vector2.down * 32f, FadeDuration).SetEase(Ease.InQuad));
            sequence.Join(_RectTransform.DOScale(_BaseScale * 0.94f, FadeDuration).SetEase(Ease.InQuad));
            sequence.Join(_CanvasGroup.DOFade(0f, FadeDuration).SetEase(Ease.InQuad));

            await UniTask.Delay(Mathf.CeilToInt(FadeDuration * 1000f), ignoreTimeScale: true);

            if (sequence.IsActive())
            {
                sequence.Kill();
            }
        }

        private void EnsureReferences()
        {
            _RectTransform ??= transform as RectTransform;
            _PreviewRectTransform ??= _PreviewImage != null ? _PreviewImage.rectTransform : null;

            if (_CanvasGroup == null)
            {
                _CanvasGroup = GetComponent<CanvasGroup>();
                if (_CanvasGroup == null)
                {
                    _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void CacheState()
        {
            if (!_HasCachedState && _RectTransform != null)
            {
                _BaseAnchoredPosition = _RectTransform.anchoredPosition;
                _BaseScale = _RectTransform.localScale;
                _HasCachedState = true;
            }

            if (!_HasCachedPreviewState && _PreviewRectTransform != null)
            {
                _PreviewBaseAnchoredPosition = _PreviewRectTransform.anchoredPosition;
                _PreviewBaseScale = _PreviewRectTransform.localScale;
                _PreviewBaseColor = _PreviewImage != null ? _PreviewImage.color : Color.white;
                _HasCachedPreviewState = true;
            }
        }

        private void PlaySwap(Action swapAction, int direction)
        {
            if (swapAction == null || _IsSwapping)
            {
                return;
            }

            EnsureReferences();
            CacheState();

            if (!gameObject.activeInHierarchy ||
                _PreviewImage == null ||
                !_PreviewImage.enabled ||
                _PreviewRectTransform == null ||
                !_HasCachedPreviewState)
            {
                swapAction();
                return;
            }

            var normalizedDirection = direction < 0 ? -1f : 1f;
            var exitOffset = Vector2.right * (-normalizedDirection * _SwapDistance);
            var entranceOffset = Vector2.right * (normalizedDirection * _SwapDistance);

            StopPreviewFloat(resetPosition: true);
            _IsSwapping = true;
            ApplyArrowAvailability();

            _PreviewRectTransform.anchoredPosition = _PreviewBaseAnchoredPosition;
            _PreviewRectTransform.localScale = _PreviewBaseScale;
            _PreviewImage.color = _PreviewBaseColor;

            _SwapSequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);
            _SwapSequence.Append(
                _PreviewRectTransform
                    .DOAnchorPos(_PreviewBaseAnchoredPosition + exitOffset, _SwapOutDuration)
                    .SetEase(Ease.InCubic));
            _SwapSequence.Join(
                _PreviewRectTransform
                    .DOScale(_PreviewBaseScale * _SwapScale, _SwapOutDuration)
                    .SetEase(Ease.InQuad));
            _SwapSequence.Join(_PreviewImage.DOFade(0f, _SwapOutDuration).SetEase(Ease.InQuad));
            _SwapSequence.AppendCallback(() =>
            {
                swapAction();
                if (_PreviewRectTransform == null || _PreviewImage == null)
                {
                    return;
                }

                _PreviewRectTransform.anchoredPosition = _PreviewBaseAnchoredPosition + entranceOffset;
                _PreviewRectTransform.localScale = _PreviewBaseScale * _SwapScale;
                _PreviewImage.color = WithAlpha(_PreviewBaseColor, 0f);
            });
            _SwapSequence.Append(
                _PreviewRectTransform
                    .DOAnchorPos(_PreviewBaseAnchoredPosition, _SwapInDuration)
                    .SetEase(Ease.OutCubic));
            _SwapSequence.Join(
                _PreviewRectTransform
                    .DOScale(_PreviewBaseScale, _SwapInDuration)
                    .SetEase(Ease.OutBack));
            _SwapSequence.Join(
                _PreviewImage
                    .DOFade(_PreviewBaseColor.a, _SwapInDuration)
                    .SetEase(Ease.OutQuad));
            _SwapSequence.OnComplete(() =>
            {
                _PreviewRectTransform.anchoredPosition = _PreviewBaseAnchoredPosition;
                _PreviewRectTransform.localScale = _PreviewBaseScale;
                _PreviewImage.color = _PreviewBaseColor;
                _SwapSequence = null;
                _IsSwapping = false;
                ApplyArrowAvailability();
                StartPreviewFloatIfNeeded();
            });
        }

        private void StopSwap(bool resetVisual, bool restartFloat)
        {
            if (_SwapSequence != null && _SwapSequence.IsActive())
            {
                _SwapSequence.Kill();
            }

            _SwapSequence = null;
            _IsSwapping = false;

            if (resetVisual && _HasCachedPreviewState && _PreviewRectTransform != null)
            {
                _PreviewRectTransform.anchoredPosition = _PreviewBaseAnchoredPosition;
                _PreviewRectTransform.localScale = _PreviewBaseScale;
                if (_PreviewImage != null)
                {
                    _PreviewImage.color = _PreviewBaseColor;
                }
            }

            ApplyArrowAvailability();
            if (restartFloat)
            {
                StartPreviewFloatIfNeeded();
            }
        }

        private void ApplyArrowAvailability()
        {
            // PlaySwap guards re-entry while the carousel is moving. Keep the
            // Selectables visually enabled so their presenter does not hide or
            // dim the arrows during the transition.
            var arrowsInteractable = _CanCycle && _CanInteract;
            if (_PreviousButton != null)
            {
                _PreviousButton.interactable = arrowsInteractable;
                _PreviousButton.gameObject.SetActive(_CanCycle);
            }

            if (_NextButton != null)
            {
                _NextButton.interactable = arrowsInteractable;
                _NextButton.gameObject.SetActive(_CanCycle);
            }
        }

        private void StartPreviewFloatIfNeeded()
        {
            EnsureReferences();
            CacheState();

            if (!gameObject.activeInHierarchy ||
                _IsSwapping ||
                _PreviewImage == null ||
                !_PreviewImage.enabled ||
                _PreviewRectTransform == null ||
                _PreviewFloatDistance <= 0f ||
                _PreviewFloatDuration <= 0f)
            {
                StopPreviewFloat(resetPosition: true);
                return;
            }

            StopPreviewFloat(resetPosition: true);

            _PreviewFloatTween = _PreviewRectTransform
                .DOAnchorPosY(_PreviewBaseAnchoredPosition.y + _PreviewFloatDistance, _PreviewFloatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(this)
                .SetUpdate(true);
        }

        private void StopPreviewFloat(bool resetPosition)
        {
            if (_PreviewFloatTween != null && _PreviewFloatTween.IsActive())
            {
                _PreviewFloatTween.Kill();
            }

            _PreviewFloatTween = null;

            if (resetPosition && _HasCachedPreviewState && _PreviewRectTransform != null)
            {
                _PreviewRectTransform.anchoredPosition = _PreviewBaseAnchoredPosition;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
