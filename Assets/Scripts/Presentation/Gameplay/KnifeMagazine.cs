using System.Collections.Generic;
using DG.Tweening;
using Presentation.Gameplay.Projectiles;
using Presentation.Gameplay.Utilities;
using Presentation.Gameplay.Views;
using UnityEngine;

namespace Presentation.Gameplay
{
    internal sealed class KnifeMagazine
    {
        private readonly KnifePool pool;
        private readonly Transform queueParent;
        private readonly Transform launcher;
        private readonly float loadBottomOffset;
        private readonly float loadAnimationDuration;
        private readonly Ease loadAnimationEase;

        private GameObject nextKnife;
        private Knife nextKnifeComponent;
        private Tween loadTween;

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
            float bottomOffset,
            float loadDuration,
            Ease loadEase)
        {
            pool = knifePool;
            queueParent = queueRoot;
            launcher = launcherTransform;
            loadBottomOffset = bottomOffset;
            loadAnimationDuration = loadDuration;
            loadAnimationEase = loadEase;
        }

        public int KnivesRemaining => knivesRemaining;
        public IReadOnlyList<GameObject> QueuedKnives => nextKnife != null ? new List<GameObject> { nextKnife } : new List<GameObject>();

        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        public void Configure(int totalKnives)
        {
            knivesRemaining = totalKnives;

            KillLoadTween();
            ReturnNextKnife();
            queueBaseRotation = launcher != null ? launcher.rotation : Quaternion.identity;
            WarmPool(totalKnives);
            LoadNextKnife();
        }

        /// <summary>
        /// Performs the TryTakeNext operation.
        /// </summary>
        public bool TryTakeNext(out GameObject knife)
        {
            knife = null;

            if (knivesRemaining <= 0 || nextKnife == null)
            {
                return false;
            }

            knife = nextKnife;
            nextKnife = null;
            nextKnifeComponent = null;
            knivesRemaining--;
            KillLoadTween();
            return knife != null;
        }

        /// <summary>
        /// Performs the TryTakeNext operation.
        /// </summary>
        public bool TryTakeNext(out GameObject knifeObject, out Knife knifeComponent)
        {
            knifeObject = null;
            knifeComponent = null;

            if (knivesRemaining <= 0 || nextKnife == null)
            {
                return false;
            }

            knifeObject = nextKnife;
            knifeComponent = nextKnifeComponent;
            nextKnife = null;
            nextKnifeComponent = null;
            knivesRemaining--;
            KillLoadTween();
            return knifeObject != null;
        }

        /// <summary>
        /// Performs the LoadNextKnife operation.
        /// </summary>
        public void LoadNextKnife()
        {
            if (knivesRemaining <= 0 || nextKnife != null)
            {
                return;
            }

            GameObject knife = pool.GetOrCreate();
            if (knife == null)
            {
                return;
            }

            nextKnife = knife;
            nextKnifeComponent = knife.GetComponent<Knife>();
            Transform knifeTransform = knife.transform;
            Vector3 launcherPosition = launcher != null ? launcher.position : queueParent.position;
            Vector3 bottomPosition = launcherPosition + Vector3.down * loadBottomOffset;
            
            knifeTransform.position = bottomPosition;
            knifeTransform.rotation = queueBaseRotation;
            knifeTransform.SetParent(queueParent);

            Vector3 targetScale = pool.PrefabOriginalScale * scaleMultiplier;
            Color startColor = pool.PrefabOriginalColor * new Color(1f, 1f, 1f, 0f);
            Color targetColor = pool.PrefabOriginalColor;

            KnifeSetupUtility.ApplyVisuals(knife, targetScale, startColor);
            KnifeSetupUtility.EnableColliders(knife, false);
            knife.SetActive(true);

            KillLoadTween();
            loadTween = DOTween.Sequence()
                .Append(knifeTransform.DOMove(launcherPosition, loadAnimationDuration).SetEase(loadAnimationEase))
                .Join(GetColorTween(knife, startColor, targetColor, loadAnimationDuration))
                .SetTarget(knife)
                .SetAutoKill(true)
                .OnKill(() => loadTween = null);
        }

        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset()
        {
            KillLoadTween();
            ReturnNextKnife();
            knivesRemaining = 0;
        }

        /// <summary>
        /// Performs the KillTweens operation.
        /// </summary>
        public void KillTweens()
        {
            KillLoadTween();
            if (nextKnife != null)
            {
                TweenKiller.Kill(nextKnife);
            }
        }

        /// <summary>
        /// Performs the SetScaleMultiplier operation.
        /// </summary>
        public void SetScaleMultiplier(float multiplier)
        {
            scaleMultiplier = Mathf.Max(0.01f, multiplier);
        }

        private void WarmPool(int totalKnives)
        {
            pool.Warmup(Mathf.Max(0, totalKnives));
        }

        private void KillLoadTween()
        {
            if (loadTween != null)
            {
                loadTween.Kill();
                loadTween = null;
            }
        }

        private void ReturnNextKnife()
        {
            if (nextKnife != null)
            {
                pool.Return(nextKnife);
                nextKnife = null;
                nextKnifeComponent = null;
            }
        }

        private Tween GetColorTween(GameObject knife, Color startColor, Color targetColor, float duration)
        {
            KnifeView view = knife.GetComponent<KnifeView>();
            if (view != null)
            {
                view.CacheIfNeeded();
                if (view.Renderer != null)
                {
                    view.Renderer.color = startColor;
                    return view.Renderer.DOColor(targetColor, duration)
                        .SetEase(loadAnimationEase)
                        .SetTarget(view.Renderer);
                }
            }

            SpriteRenderer renderer = knife.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = startColor;
                return renderer.DOColor(targetColor, duration)
                    .SetEase(loadAnimationEase)
                    .SetTarget(renderer);
            }

            return null;
        }
    }
}
