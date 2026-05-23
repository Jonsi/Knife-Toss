using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Common
{
    /// <summary>
    /// Reusable full-screen gesture hint that pulses its icon and reports the first tap.
    /// </summary>
    public class GesturePopupView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform _GestureRoot;
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private float _FadeInDuration = 0.2f;
        [SerializeField] private float _FadeOutDuration = 0.15f;
        [SerializeField] private float _PulseScale = 1.12f;
        [SerializeField] private float _PulseDuration = 0.45f;
        [SerializeField] private Vector2 _TargetOffset;

        private Sequence _PulseSequence;
        private Tween _FadeTween;
        private Vector3 _BaseScale = Vector3.one;
        private bool _IsShown;

        public event Action Tapped;

        private void Awake()
        {
            EnsureReferences();
            if (_GestureRoot != null)
            {
                _BaseScale = _GestureRoot.localScale;
            }
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        public void Show()
        {
            ShowAt(null);
        }

        public void ShowAt(RectTransform target)
        {
            ShowAt(target, true);
        }

        public void ShowAt(RectTransform target, bool blocksInput)
        {
            EnsureReferences();
            _IsShown = true;
            gameObject.SetActive(true);
            PositionGesture(target);

            if (_CanvasGroup != null)
            {
                _CanvasGroup.alpha = 0f;
                _CanvasGroup.blocksRaycasts = blocksInput;
                _CanvasGroup.interactable = blocksInput;
                _FadeTween = _CanvasGroup.DOFade(1f, _FadeInDuration).SetUpdate(true);
            }

            SetRaycastBlocking(blocksInput);
            StartPulse();
        }

        public void ShowAt(Vector2 anchoredPosition)
        {
            ShowAt(anchoredPosition, true);
        }

        public void ShowAt(Vector2 anchoredPosition, bool blocksInput)
        {
            EnsureReferences();
            _IsShown = true;
            gameObject.SetActive(true);
            PositionGesture(anchoredPosition);

            if (_CanvasGroup != null)
            {
                _CanvasGroup.alpha = 0f;
                _CanvasGroup.blocksRaycasts = blocksInput;
                _CanvasGroup.interactable = blocksInput;
                _FadeTween = _CanvasGroup.DOFade(1f, _FadeInDuration).SetUpdate(true);
            }

            SetRaycastBlocking(blocksInput);
            StartPulse();
        }

        public void Hide()
        {
            if (!_IsShown)
            {
                return;
            }

            _IsShown = false;
            KillPulse();

            if (_CanvasGroup == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _CanvasGroup.blocksRaycasts = false;
            _CanvasGroup.interactable = false;
            _FadeTween?.Kill();
            _FadeTween = _CanvasGroup
                .DOFade(0f, _FadeOutDuration)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_IsShown)
            {
                return;
            }

            Tapped?.Invoke();
            Hide();
        }

        private void EnsureReferences()
        {
            if (_CanvasGroup == null)
            {
                _CanvasGroup = GetComponent<CanvasGroup>();
            }

            if (_CanvasGroup == null)
            {
                _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_GestureRoot == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] != null && images[i].transform != transform)
                    {
                        _GestureRoot = images[i].rectTransform;
                        break;
                    }
                }
            }

            Image blocker = GetComponent<Image>();
            if (blocker != null)
            {
                blocker.raycastTarget = true;
            }
        }

        private void SetRaycastBlocking(bool blocksInput)
        {
            Image blocker = GetComponent<Image>();
            if (blocker != null)
            {
                blocker.raycastTarget = blocksInput;
            }
        }

        private void PositionGesture(RectTransform target)
        {
            if (_GestureRoot == null)
            {
                return;
            }

            if (target == null)
            {
                _GestureRoot.anchoredPosition = _TargetOffset;
                return;
            }

            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

            Canvas canvas = GetComponentInParent<Canvas>();
            Camera camera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                camera = canvas.worldCamera;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldCenter);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPoint, camera, out Vector2 localPoint))
            {
                _GestureRoot.anchoredPosition = localPoint + _TargetOffset;
            }
        }

        private void PositionGesture(Vector2 anchoredPosition)
        {
            if (_GestureRoot == null)
            {
                return;
            }

            _GestureRoot.anchoredPosition = anchoredPosition + _TargetOffset;
        }

        private void StartPulse()
        {
            if (_GestureRoot == null)
            {
                return;
            }

            KillPulse();
            _GestureRoot.localScale = _BaseScale;
            _PulseSequence = DOTween.Sequence().SetTarget(this).SetUpdate(true).SetLoops(-1);
            _PulseSequence.Append(_GestureRoot.DOScale(_BaseScale * _PulseScale, _PulseDuration).SetEase(Ease.OutQuad));
            _PulseSequence.Append(_GestureRoot.DOScale(_BaseScale, _PulseDuration).SetEase(Ease.InQuad));
        }

        private void KillTweens()
        {
            KillPulse();
            _FadeTween?.Kill();
            _FadeTween = null;
        }

        private void KillPulse()
        {
            _PulseSequence?.Kill();
            _PulseSequence = null;
        }
    }
}
