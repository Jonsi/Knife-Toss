using UnityEngine;

namespace Game.Entities.Target.Feedback
{
    /// <summary>
    /// Reveals target cracks progressively as the target takes knife hits.
    /// </summary>
    public class TargetCrackReveal : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _CracksRenderer;
        [SerializeField] private Sprite _MaskSprite;
        [SerializeField] private bool _UseSpriteMask = true;
        [SerializeField] private float _MinMaskScale = 0f;
        [SerializeField] private float _MaxMaskScale = 1f;
        [SerializeField] private int _SortingOrderOffset = 1;

        private SpriteMask _RuntimeMask;
        private Texture2D _GeneratedMaskTexture;
        private Sprite _GeneratedMaskSprite;
        private Vector3 _BaseCracksScale = Vector3.one;

        private void Awake()
        {
            ResolveRenderer();
            if (_CracksRenderer != null)
            {
                _BaseCracksScale = _CracksRenderer.transform.localScale;
            }

            EnsureMask();
            SetProgress(0f);
        }

        private void OnDestroy()
        {
            if (_GeneratedMaskSprite != null)
            {
                Destroy(_GeneratedMaskSprite);
            }

            if (_GeneratedMaskTexture != null)
            {
                Destroy(_GeneratedMaskTexture);
            }
        }

        public void ResetReveal()
        {
            SetProgress(0f);
        }

        public void SetHitProgress(int hitsReceived, int knivesToBreak)
        {
            int hitsToFullReveal = Mathf.Max(1, knivesToBreak - 1);
            float progress = (float)hitsReceived / hitsToFullReveal;
            SetProgress(progress);
        }

        public void SetSortingOrder(int sortingLayerId, int baseSortingOrder)
        {
            ResolveRenderer();
            if (_CracksRenderer == null)
                return;

            int sortingOrder = baseSortingOrder + _SortingOrderOffset;
            _CracksRenderer.sortingLayerID = sortingLayerId;
            _CracksRenderer.sortingOrder = sortingOrder;

            if (_RuntimeMask == null)
                return;

            _RuntimeMask.isCustomRangeActive = true;
            _RuntimeMask.frontSortingLayerID = sortingLayerId;
            _RuntimeMask.backSortingLayerID = sortingLayerId;
            _RuntimeMask.frontSortingOrder = sortingOrder + 1;
            _RuntimeMask.backSortingOrder = sortingOrder - 1;
        }

        private void SetProgress(float progress)
        {
            ResolveRenderer();
            if (_CracksRenderer == null)
                return;

            progress = Mathf.Clamp01(progress);
            _CracksRenderer.enabled = progress > 0f;

            float maskScale = Mathf.Lerp(_MinMaskScale, _MaxMaskScale, progress);
            if (_RuntimeMask != null)
            {
                _RuntimeMask.transform.localScale = Vector3.one * maskScale;
                _CracksRenderer.transform.localScale = _BaseCracksScale;
                return;
            }

            _CracksRenderer.transform.localScale = _BaseCracksScale * maskScale;
        }

        private void ResolveRenderer()
        {
            if (_CracksRenderer == null)
            {
                _CracksRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void EnsureMask()
        {
            if (!_UseSpriteMask || _RuntimeMask != null || _CracksRenderer == null)
                return;

            Sprite maskSprite = _MaskSprite != null ? _MaskSprite : CreateCircleMaskSprite();
            if (maskSprite == null)
                return;

            var maskObject = new GameObject("Crack Reveal Mask");
            Transform maskTransform = maskObject.transform;
            maskTransform.SetParent(_CracksRenderer.transform, false);
            maskTransform.localPosition = Vector3.zero;
            maskTransform.localRotation = Quaternion.identity;
            maskTransform.localScale = Vector3.zero;

            _RuntimeMask = maskObject.AddComponent<SpriteMask>();
            _RuntimeMask.sprite = maskSprite;
            _RuntimeMask.alphaCutoff = 0.01f;
            _CracksRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            SetSortingOrder(_CracksRenderer.sortingLayerID, _CracksRenderer.sortingOrder - _SortingOrderOffset);
        }

        private Sprite CreateCircleMaskSprite()
        {
            const int size = 64;
            const float radius = (size - 2) * 0.5f;
            var pixels = new Color32[size * size];
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    byte alpha = distance <= radius ? byte.MaxValue : (byte)0;
                    pixels[y * size + x] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, alpha);
                }
            }

            _GeneratedMaskTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Generated Crack Reveal Mask",
                hideFlags = HideFlags.DontSave
            };
            _GeneratedMaskTexture.SetPixels32(pixels);
            _GeneratedMaskTexture.Apply(false, true);

            _GeneratedMaskSprite = Sprite.Create(
                _GeneratedMaskTexture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            _GeneratedMaskSprite.name = "Generated Crack Reveal Mask";
            _GeneratedMaskSprite.hideFlags = HideFlags.DontSave;
            return _GeneratedMaskSprite;
        }
    }
}
