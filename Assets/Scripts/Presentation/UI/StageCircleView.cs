using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Represents a stage circle in the stage progress bar.
    /// </summary>
    public sealed class StageCircleView : MonoBehaviour
    {
        [SerializeField] private Image circleImage;
        [SerializeField] private TextMeshProUGUI stageNumberText;

        /// <summary>
        /// Sets the stage number.
        /// </summary>
        public void SetStage(int stageNumber)
        {
            if (stageNumberText != null)
            {
                stageNumberText.text = stageNumber.ToString();
                stageNumberText.color = Color.white;
            }

            if (circleImage != null)
            {
                circleImage.color = Color.white;
            }
        }

        /// <summary>
        /// Sets the stage circle to disabled state (no number, faded).
        /// </summary>
        public void SetDisabled(float alpha)
        {
            if (circleImage != null)
            {
                Color color = circleImage.color;
                color.a = alpha;
                circleImage.color = color;
            }

            if (stageNumberText != null)
            {
                stageNumberText.gameObject.SetActive(false);
            }
        }
    }
}

