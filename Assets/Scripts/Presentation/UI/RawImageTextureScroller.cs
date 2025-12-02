using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{




    [RequireComponent(typeof(RawImage))]
    public sealed class RawImageTextureScroller : MonoBehaviour
    {
        [SerializeField] private RawImage targetImage;
        [SerializeField] private Vector2 scrollSpeed = new Vector2(-0.1f, 0f);
        [SerializeField] private Vector2 initialOffset = Vector2.zero;
        [SerializeField] private bool useUnscaledTime;

        private Rect uvRect;

        private void Awake()
        {
            targetImage ??= GetComponent<RawImage>();
            if (targetImage == null)
            {
                enabled = false;
                return;
            }

            uvRect = targetImage.uvRect;
            uvRect.position = initialOffset;
            targetImage.uvRect = uvRect;
        }

        private void Update()
        {
            if (targetImage == null)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            uvRect.position += scrollSpeed * deltaTime;
            targetImage.uvRect = uvRect;
        }
    }
}
