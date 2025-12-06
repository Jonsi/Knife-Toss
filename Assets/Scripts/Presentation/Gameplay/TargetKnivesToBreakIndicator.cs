using App.Services;
using Core.Domain;
using TMPro;
using UnityEngine;
using VContainer;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Displays how many knives are still required to break a target.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TargetKnivesToBreakIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshPro knivesLabel;

        private int knivesToBreak;
        private int remaining;
        private LevelSession session;
        private bool isDestroyed;

        private void OnEnable()
        {
            TargetKnivesToBreakIndicatorRegistry.Register(this);
        }

        private void OnDisable()
        {
            TargetKnivesToBreakIndicatorRegistry.Unregister(this);
        }

        /// <summary>
        /// Injects dependencies via VContainer.
        /// </summary>
        [Inject]
        public void Construct(LevelSessionService sessionService)
        {
            session = sessionService?.Session;
        }

        /// <summary>
        /// Configures the indicator for a newly spawned target.
        /// </summary>
        public void Configure(int knivesToBreak)
        {
            this.knivesToBreak = Mathf.Max(0, knivesToBreak);
            remaining = this.knivesToBreak;
            UpdateLabel();
        }

        /// <summary>
        /// Reduces the remaining count after a successful knife hit.
        /// </summary>
        public void RegisterHit()
        {
            if (isDestroyed)
            {
                return;
            }

            remaining = Mathf.Max(0, remaining - 1);
            UpdateLabel();

            // If knives to break is complete, destroy the target
            if (knivesToBreak > 0 && remaining <= 0)
            {
                DestroyTarget();
            }
        }

        /// <summary>
        /// Destroys the target when the quota is reached.
        /// </summary>
        private void DestroyTarget()
        {
            if (isDestroyed)
            {
                return;
            }

            isDestroyed = true;

            // Notify session that target was destroyed
            session?.MarkTargetCompleted();

            // Check if level should complete
            session?.MarkCompleteIfNeeded();

            // Destroy the target GameObject
            if (transform != null)
            {
                // Find the root target GameObject by traversing up the hierarchy
                Transform targetRoot = transform;
                while (targetRoot != null)
                {
                    if (targetRoot.CompareTag("Target"))
                    {
                        // Kill all tweens on the target and its children before destroying
                        KillAllTweensOnTarget(targetRoot.gameObject);
                        Destroy(targetRoot.gameObject);
                        return;
                    }
                    targetRoot = targetRoot.parent;
                }
                
                // Fallback: destroy this component's GameObject if no target root found
                KillAllTweensOnTarget(gameObject);
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Kills all tweens on the target GameObject and all its children.
        /// </summary>
        private static void KillAllTweensOnTarget(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            // Kill tweens on the target itself
            TweenKiller.Kill(target);

            // Kill tweens on all children recursively
            KillTweensOnChildren(target.transform);
        }

        /// <summary>
        /// Recursively kills all tweens on children transforms.
        /// </summary>
        private static void KillTweensOnChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null)
                {
                    TweenKiller.Kill(child.gameObject);
                    KillTweensOnChildren(child);
                }
            }
        }

        private void UpdateLabel()
        {
            if (knivesLabel == null)
            {
                return;
            }

            knivesLabel.gameObject.SetActive(true);
            knivesLabel.text = Mathf.Max(0, remaining).ToString();
        }
    }
}

