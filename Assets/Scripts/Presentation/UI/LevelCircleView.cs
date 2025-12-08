using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Represents a single level circle in the stage progress bar.
    /// </summary>
    public sealed class LevelCircleView : MonoBehaviour
    {
        [SerializeField] private Image greenFillImage;

        /// <summary>
        /// Sets whether this level circle is filled (completed/current).
        /// </summary>
        public void SetFilled(bool filled)
        {
            if (greenFillImage != null)
            {
                greenFillImage.gameObject.SetActive(filled);
            }
        }
    }
}

