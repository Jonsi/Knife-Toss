using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Common
{
    /// <summary>
    /// Spawns lightweight UI coin icons that travel toward a destination rect.
    /// </summary>
    public sealed class CurrencyBurstAnimator
    {
        private readonly Canvas _Canvas;
        private readonly RectTransform _Destination;
        private readonly CurrencyAccumulationConfig _Config;

        private RectTransform _OverlayRoot;

        public CurrencyBurstAnimator(Canvas canvas, RectTransform destination, CurrencyAccumulationConfig config)
        {
            _Canvas = canvas;
            _Destination = destination;
            _Config = config;
        }

        public bool IsValid => _Canvas != null && _Destination != null && _Config != null && _Config.IconSprite != null;

        public void PlayFromScreenPoint(Vector2 screenPoint, int amount)
        {
            if (!IsValid || amount <= 0)
            {
                return;
            }

            RectTransform overlayRoot = EnsureOverlayRoot();
            if (overlayRoot == null)
            {
                return;
            }

            Camera uiCamera = _Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _Canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRoot, screenPoint, uiCamera, out Vector2 startPoint))
            {
                return;
            }

            Vector2 destinationPoint = GetDestinationPoint(overlayRoot, uiCamera);
            int coinCount = Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Sqrt(amount)) + 2,
                _Config.MinCoinCount,
                _Config.MaxCoinCount);

            for (int i = 0; i < coinCount; i++)
            {
                SpawnCoin(overlayRoot, startPoint, destinationPoint, i);
            }
        }

        private RectTransform EnsureOverlayRoot()
        {
            if (_OverlayRoot != null)
            {
                return _OverlayRoot;
            }

            var rootObject = new GameObject("CurrencyBurstOverlay", typeof(RectTransform));
            _OverlayRoot = rootObject.GetComponent<RectTransform>();
            _OverlayRoot.SetParent(_Canvas.transform, false);
            _OverlayRoot.anchorMin = Vector2.zero;
            _OverlayRoot.anchorMax = Vector2.one;
            _OverlayRoot.offsetMin = Vector2.zero;
            _OverlayRoot.offsetMax = Vector2.zero;
            _OverlayRoot.SetAsLastSibling();
            return _OverlayRoot;
        }

        private Vector2 GetDestinationPoint(RectTransform overlayRoot, Camera uiCamera)
        {
            Vector2 destinationScreenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, _Destination.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRoot, destinationScreenPoint, uiCamera, out Vector2 destinationPoint)
                ? destinationPoint
                : Vector2.zero;
        }

        private void SpawnCoin(RectTransform parent, Vector2 startPoint, Vector2 destinationPoint, int index)
        {
            var coinObject = new GameObject("CoinBurstFx", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rectTransform = coinObject.GetComponent<RectTransform>();
            var canvasGroup = coinObject.GetComponent<CanvasGroup>();
            var image = coinObject.GetComponent<Image>();

            rectTransform.SetParent(parent, false);
            rectTransform.sizeDelta = Vector2.one * _Config.IconSize;
            rectTransform.anchoredPosition = startPoint;
            rectTransform.localScale = Vector3.one * Random.Range(0.85f, 1.1f);

            image.sprite = _Config.IconSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            canvasGroup.alpha = 0f;

            Vector2 scatterOffset = Random.insideUnitCircle * _Config.ScatterRadius;
            Vector2 arcPoint = startPoint + scatterOffset;
            float delay = index * _Config.SpawnDelayStep;
            float travelDuration = _Config.TravelDuration + index * _Config.TravelDurationStep;

            Sequence sequence = DOTween.Sequence();
            sequence.SetLink(coinObject);
            sequence.AppendInterval(delay);
            sequence.Append(canvasGroup.DOFade(1f, 0.08f));
            sequence.Join(rectTransform.DOAnchorPos(arcPoint, _Config.PopDuration).SetEase(Ease.OutQuad));
            sequence.Join(rectTransform.DOScale(1.2f, _Config.PopDuration).SetEase(Ease.OutBack));
            sequence.Append(rectTransform.DOAnchorPos(destinationPoint, travelDuration).SetEase(Ease.InQuad));
            sequence.Join(rectTransform.DOScale(_Config.EndScale, travelDuration).SetEase(Ease.InBack));
            float fadeStartTime = travelDuration * _Config.FadeStartNormalized;
            float fadeDuration = Mathf.Max(0.01f, travelDuration - fadeStartTime);
            sequence.Join(canvasGroup.DOFade(_Config.FinalAlpha, fadeDuration).SetDelay(fadeStartTime));
            sequence.OnComplete(() => Object.Destroy(coinObject));
            sequence.OnKill(() =>
            {
                if (coinObject != null)
                {
                    Object.Destroy(coinObject);
                }
            });
        }
    }
}
