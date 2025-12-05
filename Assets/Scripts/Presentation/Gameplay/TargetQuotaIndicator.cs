using TMPro;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Displays how many knives are still required for a target.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TargetQuotaIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshPro quotaLabel;
        [SerializeField] private Color pendingColor = Color.white;
        [SerializeField] private Color completeColor = Color.green;
        [SerializeField] private bool hideWhenComplete = true;

        private int totalQuota;
        private int remaining;
        private bool isActive;

        private void OnEnable()
        {
            TargetQuotaIndicatorRegistry.Register(this);
        }

        private void OnDisable()
        {
            TargetQuotaIndicatorRegistry.Unregister(this);
        }

        /// <summary>
        /// Configures the indicator for a newly spawned target.
        /// </summary>
        public void Configure(int quota)
        {
            totalQuota = Mathf.Max(0, quota);
            remaining = totalQuota;
            isActive = totalQuota > 0;
            UpdateLabel();
        }

        /// <summary>
        /// Reduces the remaining count after a successful knife hit.
        /// </summary>
        public void RegisterHit()
        {
            if (!isActive)
            {
                return;
            }

            remaining = Mathf.Max(0, remaining - 1);
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (quotaLabel == null)
            {
                return;
            }

            if (!isActive)
            {
                quotaLabel.gameObject.SetActive(false);
                return;
            }

            bool completed = remaining <= 0;
            if (hideWhenComplete && completed)
            {
                quotaLabel.gameObject.SetActive(false);
                return;
            }

            quotaLabel.gameObject.SetActive(true);
            quotaLabel.text = Mathf.Max(0, remaining).ToString();
            quotaLabel.color = completed ? completeColor : pendingColor;
        }
    }
}

