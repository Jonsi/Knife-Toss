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

        private RectTransform _RectTransform;
        private RectTransform _PreviewRectTransform;
        private Vector2 _BaseAnchoredPosition;
        private Vector2 _PreviewBaseAnchoredPosition;
        private Vector3 _BaseScale;
        private bool _HasCachedState;
        private bool _HasCachedPreviewState;
        private Tween _PreviewFloatTween;

        private void Awake()
        {
            EnsureReferences();
            CacheState();
        }

        private void OnDisable()
        {
            StopPreviewFloat(resetPosition: true);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        public void Bind(Action onPrevious, Action onNext)
        {
            if (_PreviousButton != null)
            {
                _PreviousButton.onClick.RemoveAllListeners();
                if (onPrevious != null)
                {
                    _PreviousButton.onClick.AddListener(() => onPrevious());
                }
            }

            if (_NextButton != null)
            {
                _NextButton.onClick.RemoveAllListeners();
                if (onNext != null)
                {
                    _NextButton.onClick.AddListener(() => onNext());
                }
            }
        }

        public void SetDisplay(Sprite previewSprite, bool canCycle, bool canInteract)
        {
            if (_PreviewImage != null)
            {
                _PreviewImage.sprite = previewSprite;
                _PreviewImage.enabled = previewSprite != null;
            }

            StartPreviewFloatIfNeeded();

            var arrowsInteractable = canCycle && canInteract;
            if (_PreviousButton != null)
            {
                _PreviousButton.interactable = arrowsInteractable;
                _PreviousButton.gameObject.SetActive(canCycle);
            }

            if (_NextButton != null)
            {
                _NextButton.interactable = arrowsInteractable;
                _NextButton.gameObject.SetActive(canCycle);
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
                _HasCachedPreviewState = true;
            }
        }

        private void StartPreviewFloatIfNeeded()
        {
            EnsureReferences();
            CacheState();

            if (!gameObject.activeInHierarchy ||
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
    }
}
