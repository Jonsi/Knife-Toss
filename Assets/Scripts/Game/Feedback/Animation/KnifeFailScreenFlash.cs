using UnityEngine;
using UnityEngine.UI;

namespace Game.Feedback.Animation
{
    /// <summary>
    /// Runtime-created full-screen white overlay used for knife fail flash feedback.
    /// </summary>
    internal sealed class KnifeFailScreenFlash
    {
        private static KnifeFailScreenFlash _Instance;

        private readonly CanvasGroup _CanvasGroup;

        private KnifeFailScreenFlash()
        {
            var root = new GameObject("KnifeFailScreenFlash");
            Object.DontDestroyOnLoad(root);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            root.AddComponent<GraphicRaycaster>();
            _CanvasGroup = root.AddComponent<CanvasGroup>();
            _CanvasGroup.alpha = 0f;
            _CanvasGroup.blocksRaycasts = false;
            _CanvasGroup.interactable = false;

            var imageObject = new GameObject("Flash");
            imageObject.transform.SetParent(root.transform, false);

            var image = imageObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;

            var rectTransform = image.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static KnifeFailScreenFlash Instance => _Instance ??= new KnifeFailScreenFlash();

        public void SetAlpha(float alpha)
        {
            if (_CanvasGroup == null)
                return;

            _CanvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        public void Hide()
        {
            SetAlpha(0f);
        }
    }
}
