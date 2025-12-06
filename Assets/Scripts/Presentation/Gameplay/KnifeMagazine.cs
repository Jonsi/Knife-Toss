using System.Collections.Generic;
using DG.Tweening;
using Presentation.Gameplay.Utilities;
using UnityEngine;

namespace Presentation.Gameplay
{
    internal sealed class KnifeMagazine
    {
        private readonly KnifePool pool;
        private readonly Transform queueParent;
        private readonly Transform launcher;
        private readonly int visibleQueueCount;
        private readonly float queueSpacing;
        private readonly float slideAnimationDuration;
        private readonly float queueScaleEnd;
        private readonly float queueOpacityEnd;

        private readonly List<GameObject> queue = new List<GameObject>();

        private int knivesRemaining;
        private float scaleMultiplier = 1f;
        private Quaternion queueBaseRotation = Quaternion.identity;

        /// <summary>
        /// Performs the KnifeMagazine operation.
        /// </summary>
        public KnifeMagazine(
            KnifePool knifePool,
            Transform queueRoot,
            Transform launcherTransform,
            int visibleCount,
            float spacing,
            float slideDuration,
            float scaleEnd,
            float opacityEnd)
        {
            pool = knifePool;
            queueParent = queueRoot;
            launcher = launcherTransform;
            visibleQueueCount = Mathf.Max(1, visibleCount);
            queueSpacing = spacing;
            slideAnimationDuration = slideDuration;
            queueScaleEnd = scaleEnd;
            queueOpacityEnd = opacityEnd;
        }

        public int KnivesRemaining => knivesRemaining;
        public IReadOnlyList<GameObject> QueuedKnives => queue;

        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        public void Configure(int totalKnives)
        {
            knivesRemaining = totalKnives;

            pool.ReturnAll(queue);
            ClearQueue();
            queueBaseRotation = launcher != null ? launcher.rotation : Quaternion.identity;
            WarmPool(totalKnives);
            FillInitialQueue();
        }

        /// <summary>
        /// Performs the TryTakeNext operation.
        /// </summary>
        public bool TryTakeNext(out GameObject knife)
        {
            knife = null;

            if (knivesRemaining <= 0 || queue.Count == 0)
            {
                return false;
            }

            knife = queue[0];
            queue.RemoveAt(0);
            knivesRemaining--;
            return knife != null;
        }

        /// <summary>
        /// Performs the SlideQueueForward operation.
        /// </summary>
        public void SlideQueueForward()
        {
            AnimateQueue();
            MaintainQueueSize();
            UpdateQueueVisibility();
        }

        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset()
        {
            pool.ReturnAll(queue);
            ClearQueue();
            knivesRemaining = 0;
        }

        /// <summary>
        /// Performs the KillTweens operation.
        /// </summary>
        public void KillTweens()
        {
            TweenKiller.Kill(queue);
        }

        /// <summary>
        /// Performs the AlignFirstKnife operation.
        /// </summary>
        public void AlignFirstKnife(Vector2 direction)
        {
            if (queue.Count == 0)
            {
                return;
            }

            GameObject knife = queue[0];
            if (knife == null)
            {
                return;
            }

            Transform knifeTransform = knife.transform;
            knifeTransform.up = direction;
        }

        /// <summary>
        /// Performs the SetScaleMultiplier operation.
        /// </summary>
        public void SetScaleMultiplier(float multiplier)
        {
            scaleMultiplier = Mathf.Max(0.01f, multiplier);
        }

        /// <summary>
        /// Performs the WarmPool operation.
        /// </summary>
        private void WarmPool(int totalKnives)
        {
            pool.Warmup(Mathf.Max(0, totalKnives));
        }

        /// <summary>
        /// Performs the FillInitialQueue operation.
        /// </summary>
        private void FillInitialQueue()
        {
            int targetCount = Mathf.Min(visibleQueueCount + 1, knivesRemaining);
            for (int i = 0; i < targetCount; i++)
            {
                AddKnifeToQueue();
            }

            UpdateQueueVisibility();
        }

        /// <summary>
        /// Performs the AnimateQueue operation.
        /// </summary>
        private void AnimateQueue()
        {
            for (int i = 0; i < queue.Count; i++)
            {
                GameObject knife = queue[i];
                if (knife == null)
                {
                    continue;
                }

                TweenKiller.Kill(knife);

                Transform knifeTransform = knife.transform;
                if (knifeTransform == null)
                {
                    continue;
                }

                SpriteRenderer renderer = knife.GetComponent<SpriteRenderer>();

                Vector3 targetPosition = GetQueuePosition(i);
                Vector3 targetScale = pool.PrefabOriginalScale * GetQueueScaleMultiplier(i) * ResolveScaleMultiplier();
                Color targetColor = pool.PrefabOriginalColor * new Color(1f, 1f, 1f, GetQueueOpacityMultiplier(i));

                knifeTransform.DOMove(targetPosition, slideAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(knifeTransform)
                    .SetAutoKill(true);
                
                knifeTransform.DOScale(targetScale, slideAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(knifeTransform)
                    .SetAutoKill(true);

                if (renderer != null)
                {
                    renderer.DOColor(targetColor, slideAnimationDuration)
                        .SetEase(Ease.OutQuad)
                        .SetTarget(renderer)
                        .SetAutoKill(true);
                }
            }
        }

        /// <summary>
        /// Performs the MaintainQueueSize operation.
        /// </summary>
        private void MaintainQueueSize()
        {
            int targetCount = Mathf.Min(visibleQueueCount + 1, knivesRemaining);
            while (queue.Count < targetCount)
            {
                AddKnifeToQueue();
            }
        }

        /// <summary>
        /// Performs the AddKnifeToQueue operation.
        /// </summary>
        private void AddKnifeToQueue()
        {
            GameObject knife = pool.GetOrCreate();
            if (knife == null)
            {
                return;
            }

            int index = queue.Count;

            Transform knifeTransform = knife.transform;
            knifeTransform.position = GetQueuePosition(index);
            knifeTransform.rotation = queueBaseRotation;
            Vector3 targetScale = pool.PrefabOriginalScale * GetQueueScaleMultiplier(index) * ResolveScaleMultiplier();
            knifeTransform.SetParent(queueParent);

            Color targetColor = ResolveQueueColor(index);
            KnifeSetupUtility.ApplyVisuals(knife, targetScale, targetColor);
            KnifeSetupUtility.EnableColliders(knife, false);
            knife.SetActive(true);
            queue.Add(knife);
        }

        /// <summary>
        /// Performs the GetQueuePosition operation.
        /// </summary>
        private Vector3 GetQueuePosition(int queueIndex)
        {
            Vector3 origin = launcher != null ? launcher.position : queueParent.position;
            float offsetX = -queueIndex * queueSpacing;
            return origin + Vector3.right * offsetX;
        }

        /// <summary>
        /// Performs the GetQueueScaleMultiplier operation.
        /// </summary>
        private float GetQueueScaleMultiplier(int queueIndex)
        {
            if (visibleQueueCount <= 1)
            {
                return 1f;
            }

            float t = Mathf.Clamp01((float)queueIndex / (visibleQueueCount - 1));
            return Mathf.Lerp(1f, queueScaleEnd, t);
        }

        /// <summary>
        /// Performs the GetQueueOpacityMultiplier operation.
        /// </summary>
        private float GetQueueOpacityMultiplier(int queueIndex)
        {
            if (queueIndex >= visibleQueueCount)
            {
                return 0f;
            }

            if (visibleQueueCount <= 1)
            {
                return 1f;
            }

            float t = Mathf.Clamp01((float)queueIndex / (visibleQueueCount - 1));
            return Mathf.Lerp(1f, queueOpacityEnd, t);
        }

        /// <summary>
        /// Performs the UpdateQueueVisibility operation.
        /// </summary>
        private void UpdateQueueVisibility()
        {
            for (int i = 0; i < queue.Count; i++)
            {
                GameObject knife = queue[i];
                if (knife == null)
                {
                    continue;
                }

                Color targetColor = ResolveQueueColor(i);
                KnifeSetupUtility.ApplyVisuals(knife, knife.transform.localScale, targetColor);

                knife.SetActive(true);
            }
        }

        /// <summary>
        /// Performs the ClearQueue operation.
        /// </summary>
        private void ClearQueue()
        {
            queue.Clear();
        }

        /// <summary>
        /// Performs the ResolveQueueColor operation.
        /// </summary>
        private Color ResolveQueueColor(int index)
        {
            float opacity = GetQueueOpacityMultiplier(index);
            return pool.PrefabOriginalColor * new Color(1f, 1f, 1f, opacity);
        }

        /// <summary>
        /// Performs the ResolveScaleMultiplier operation.
        /// </summary>
        private float ResolveScaleMultiplier()
        {
            return scaleMultiplier;
        }
    }
}
