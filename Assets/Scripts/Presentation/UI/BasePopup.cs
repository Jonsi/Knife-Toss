using UnityEngine;

namespace Presentation.UI
{
    /// <summary>
    /// Base class for all popups in the game. Provides common show/hide functionality.
    /// </summary>
    public abstract class BasePopup : MonoBehaviour
    {
        /// <summary>
        /// Shows the popup.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

