using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Maintains the queued knives, handles layout, and provides knives for throwing.
    /// </summary>
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

        public void Configure(int totalKnives)
        {
            knivesRemaining = totalKnives;

            pool.ReturnAll(queue);
            ClearQueue();
            WarmPool(totalKnives);
            FillInitialQueue();
        }

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

        public void SlideQueueForward()
        {
            AnimateQueue();
            MaintainQueueSize();
            UpdateQueueVisibility();
        }

        public void Reset()
        {
            pool.ReturnAll(queue);
            ClearQueue();
            knivesRemaining = 0;
        }

        public void KillTweens()
        {
            TweenKiller.Kill(queue);
        }

        private void WarmPool(int totalKnives)
        {
            pool.Warmup(Mathf.Max(0, totalKnives));
        }

        private void FillInitialQueue()
        {
            int targetCount = Mathf.Min(visibleQueueCount + 1, knivesRemaining);
            for (int i = 0; i < targetCount; i++)
            {
                AddKnifeToQueue();
            }

            UpdateQueueVisibility();
        }

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
                SpriteRenderer renderer = knife.GetComponent<SpriteRenderer>();

                Vector3 targetPosition = GetQueuePosition(i);
                Vector3 targetScale = pool.PrefabOriginalScale * GetQueueScaleMultiplier(i);
                Color targetColor = pool.PrefabOriginalColor * new Color(1f, 1f, 1f, GetQueueOpacityMultiplier(i));

                knifeTransform.DOMove(targetPosition, slideAnimationDuration).SetEase(Ease.OutQuad);
                knifeTransform.DOScale(targetScale, slideAnimationDuration).SetEase(Ease.OutQuad);

                if (renderer != null)
                {
                    renderer.DOColor(targetColor, slideAnimationDuration).SetEase(Ease.OutQuad);
                }
            }
        }

        private void MaintainQueueSize()
        {
            int targetCount = Mathf.Min(visibleQueueCount + 1, knivesRemaining);
            while (queue.Count < targetCount)
            {
                AddKnifeToQueue();
            }
        }

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
            knifeTransform.rotation = launcher != null ? launcher.rotation : Quaternion.identity;
            knifeTransform.localScale = pool.PrefabOriginalScale * GetQueueScaleMultiplier(index);
            knifeTransform.SetParent(queueParent);

            SpriteRenderer renderer = knife.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = pool.PrefabOriginalColor * new Color(1f, 1f, 1f, GetQueueOpacityMultiplier(index));
            }

            DisableColliders(knife);
            knife.SetActive(true);
            queue.Add(knife);
        }

        private Vector3 GetQueuePosition(int queueIndex)
        {
            Vector3 origin = launcher != null ? launcher.position : queueParent.position;
            float offsetX = -queueIndex * queueSpacing;
            return origin + Vector3.right * offsetX;
        }

        private float GetQueueScaleMultiplier(int queueIndex)
        {
            if (visibleQueueCount <= 1)
            {
                return 1f;
            }

            float t = Mathf.Clamp01((float)queueIndex / (visibleQueueCount - 1));
            return Mathf.Lerp(1f, queueScaleEnd, t);
        }

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

        private void UpdateQueueVisibility()
        {
            for (int i = 0; i < queue.Count; i++)
            {
                GameObject knife = queue[i];
                if (knife == null)
                {
                    continue;
                }

                SpriteRenderer renderer = knife.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    float opacity = GetQueueOpacityMultiplier(i);
                    renderer.color = pool.PrefabOriginalColor * new Color(1f, 1f, 1f, opacity);
                }

                knife.SetActive(true);
            }
        }

        private void ClearQueue()
        {
            queue.Clear();
        }

        private static void DisableColliders(GameObject knife)
        {
            if (knife == null)
            {
                return;
            }

            Collider2D[] colliders = knife.GetComponents<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }
}
