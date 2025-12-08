using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Represents a single slot in the stage progress bar.
    /// </summary>
    public sealed class ProgressBarSlot : MonoBehaviour
    {
        [SerializeField] private Image slotImage;

        /// <summary>
        /// Sets the slot color based on completion status.
        /// </summary>
        public void SetCompleted(bool completed, Color completeColor, Color incompleteColor)
        {
            if (slotImage == null) return;
            slotImage.color = completed ? completeColor : incompleteColor;
        }
    }
}
