#if TEST_ENABLED
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Debugging
{
    public class SRDebuggerToggleButton : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        private RectTransform _RectTransform;
        private Canvas _Canvas;
        private bool _IsDragging;

        private void Awake()
        {
#if DISABLE_SRDEBUGGER
            Destroy(gameObject);
            return;
#endif
            _RectTransform = GetComponent<RectTransform>();
            _Canvas = GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_Canvas == null) return;
            
            _IsDragging = true;
            _RectTransform.anchoredPosition += eventData.delta / _Canvas.scaleFactor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // If we were dragging, ignore the click
            if (_IsDragging)
            {
                _IsDragging = false;
                return;
            }

            if (SRDebug.Instance.IsDebugPanelVisible)
                SRDebug.Instance.HideDebugPanel();
            else
                SRDebug.Instance.ShowDebugPanel();
        }
    }
}
#endif