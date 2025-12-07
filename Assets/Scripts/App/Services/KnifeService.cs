using System.Collections.Generic;
using Core.Abstractions;
using DG.Tweening;
using Presentation.Gameplay;
using Presentation.Gameplay.Projectiles;
using Presentation.Gameplay.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace App.Services
{
    /// <summary>
    /// Represents the KnifeService class.
    /// </summary>
    public class KnifeService : IKnifeService
    {
        private readonly KnifePool _knifePool;
        private readonly KnifeMagazine _magazine;

        /// <summary>
        /// Performs the KnifeService operation.
        /// </summary>
        public KnifeService(
            GameObject knifePrefab,
            Transform defaultParent,
            Transform launcherTransform,
            IObjectResolver objectResolver,
            int poolWarmupCount = 10,
            float loadBottomOffset = 0.5f,
            float loadAnimationDuration = 0.3f,
            Ease loadAnimationEase = Ease.OutQuad)
        {
            _knifePool = new KnifePool(knifePrefab, defaultParent, poolWarmupCount, objectResolver);
            _magazine = new KnifeMagazine(
                _knifePool,
                defaultParent,
                launcherTransform,
                loadBottomOffset,
                loadAnimationDuration,
                loadAnimationEase);
        }

        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        public void Configure(int totalKnives) => _magazine.Configure(totalKnives);
        /// <summary>
        /// Performs the GetNextKnife operation.
        /// </summary>
        public Knife GetNextKnife() => _magazine.TryTakeNext(out var knifeObject) ? knifeObject.GetComponent<Knife>() : null;
        /// <summary>
        /// Performs the Return operation.
        /// </summary>
        public void Return(Knife knife) => _knifePool.Return(knife.gameObject);
        /// <summary>
        /// Performs the SlideQueueForward operation.
        /// </summary>
        public void SlideQueueForward() => _magazine.LoadNextKnife();
        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset() => _magazine.Reset();

        private sealed class KnifeMagazine
        {
            private readonly KnifePool _pool;
            private readonly Transform _queueParent;
            private readonly Transform _launcher;
            private readonly float _loadBottomOffset;
            private readonly float _loadAnimationDuration;
            private readonly Ease _loadAnimationEase;

            private GameObject _nextKnife;
            private Tween _loadTween;

            public int KnivesRemaining { get; private set; }

            /// <summary>
            /// Performs the KnifeMagazine operation.
            /// </summary>
            public KnifeMagazine(KnifePool knifePool, Transform queueRoot, Transform launcherTransform, float bottomOffset, float loadDuration, Ease loadEase)
            {
                _pool = knifePool;
                _queueParent = queueRoot;
                _launcher = launcherTransform;
                _loadBottomOffset = bottomOffset;
                _loadAnimationDuration = loadDuration;
                _loadAnimationEase = loadEase;
            }

            /// <summary>
            /// Performs the Configure operation.
            /// </summary>
            public void Configure(int totalKnives)
            {
                KillLoadTween();
                ReturnNextKnife();
                KnivesRemaining = totalKnives;
                _pool.Warmup(Mathf.Max(0, totalKnives));
                LoadNextKnife();
            }

            /// <summary>
            /// Performs the TryTakeNext operation.
            /// </summary>
            public bool TryTakeNext(out GameObject knife)
            {
                knife = null;
                if (KnivesRemaining <= 0 || _nextKnife == null) return false;

                var knifeView = _nextKnife.GetComponent<KnifeView>();
                knifeView?.KillTweens();
                knife = _nextKnife;
                _nextKnife = null;
                KnivesRemaining--;
                KillLoadTween();
                return knife != null;
            }

            /// <summary>
            /// Performs the LoadNextKnife operation.
            /// </summary>
            public void LoadNextKnife()
            {
                if (KnivesRemaining <= 0 || _nextKnife != null) return;

                var knife = _pool.GetOrCreate();
                if (knife == null) return;

                _nextKnife = knife;
                var knifeTransform = knife.transform;
                var launcherPosition = _launcher != null ? _launcher.position : _queueParent.position;
                var bottomPosition = launcherPosition + Vector3.down * _loadBottomOffset;

                knifeTransform.position = bottomPosition;
                knifeTransform.rotation = _launcher != null ? _launcher.rotation : Quaternion.identity;
                knifeTransform.SetParent(_queueParent);

                var targetScale = _pool.PrefabOriginalScale;
                var startColor = _pool.PrefabOriginalColor * new Color(1f, 1f, 1f, 0f);
                var targetColor = _pool.PrefabOriginalColor;

                var view = knife.GetComponent<KnifeView>();
                if (view != null)
                {
                    view.CacheIfNeeded();
                    view.ApplyVisuals(bottomPosition, targetScale, startColor, knifeTransform.rotation);
                    view.SetColliders(false);
                }
                else
                {
                    knifeTransform.localScale = targetScale;
                    var renderer = knife.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = startColor;
                    }
                }

                knife.SetActive(true);

                KillLoadTween();
                _loadTween = DOTween.Sequence()
                    .Append(knifeTransform.DOMove(launcherPosition, _loadAnimationDuration).SetEase(_loadAnimationEase))
                    .Join(GetColorTween(knife, startColor, targetColor, _loadAnimationDuration))
                    .SetTarget(knife)
                    .SetAutoKill(true)
                    .OnKill(() => _loadTween = null);
            }

            /// <summary>
            /// Performs the Reset operation.
            /// </summary>
            public void Reset()
            {
                KillLoadTween();
                ReturnNextKnife();
                KnivesRemaining = 0;
            }

            /// <summary>
            /// Performs the KillTweens operation.
            /// </summary>
            public void KillTweens()
            {
                KillLoadTween();
                if (_nextKnife != null)
                {
                    _nextKnife.GetComponent<KnifeView>()?.KillTweens();
                }
            }

            private void KillLoadTween()
            {
                if (_loadTween != null)
                {
                    _loadTween.Kill();
                    _loadTween = null;
                }
            }

            private void ReturnNextKnife()
            {
                if (_nextKnife != null)
                {
                    _pool.Return(_nextKnife);
                    _nextKnife = null;
                }
            }

            private Tween GetColorTween(GameObject knife, Color startColor, Color targetColor, float duration)
            {
                var view = knife.GetComponent<KnifeView>();
                if (view != null && view.Renderer != null)
                {
                    view.Renderer.color = startColor;
                    return view.Renderer.DOColor(targetColor, duration)
                        .SetEase(_loadAnimationEase)
                        .SetTarget(view.Renderer);
                }

                var renderer = knife.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = startColor;
                    return renderer.DOColor(targetColor, duration)
                        .SetEase(_loadAnimationEase)
                        .SetTarget(renderer);
                }

                return null;
            }
        }

        private sealed class KnifePool
        {
            private readonly GameObject _prefab;
            private readonly Transform _defaultParent;
            private readonly Queue<GameObject> _pool = new Queue<GameObject>();
            private readonly IObjectResolver _resolver;
            public Vector3 PrefabOriginalScale { get; }
            public Color PrefabOriginalColor { get; }

            /// <summary>
            /// Performs the KnifePool operation.
            /// </summary>
            public KnifePool(GameObject knifePrefab, Transform parent, int warmupCount, IObjectResolver objectResolver)
            {
                _prefab = knifePrefab;
                _defaultParent = parent;
                _resolver = objectResolver;

                if (_prefab != null)
                {
                    PrefabOriginalScale = _prefab.transform.localScale;
                    var renderer = _prefab.GetComponent<SpriteRenderer>();
                    PrefabOriginalColor = renderer != null ? renderer.color : Color.white;
                }
                else
                {
                    PrefabOriginalScale = Vector3.one;
                    PrefabOriginalColor = Color.white;
                }
                Warmup(warmupCount);
            }

            /// <summary>
            /// Performs the Warmup operation.
            /// </summary>
            public void Warmup(int count)
            {
                if (_prefab == null) return;
                while (_pool.Count < count)
                {
                    _pool.Enqueue(CreateKnife());
                }
            }

            /// <summary>
            /// Performs the GetOrCreate operation.
            /// </summary>
            public GameObject GetOrCreate()
            {
                if (_prefab == null) return null;
                if (_pool.Count == 0) Warmup(1);
                return _pool.Count > 0 ? _pool.Dequeue() : CreateKnife();
            }

            /// <summary>
            /// Performs the Return operation.
            /// </summary>
            public void Return(GameObject knife)
            {
                if (knife == null) return;
                var view = knife.GetComponent<KnifeView>();
                if (view == null)
                {
                    Object.Destroy(knife);
                    return;
                }
                
                view.KillTweens();
                view.ResetTransform(_defaultParent, PrefabOriginalScale);
                view.SetColor(PrefabOriginalColor);
                view.SetColliders(false);
                knife.SetActive(false);
                _pool.Enqueue(knife);
            }

            /// <summary>
            /// Performs the ReturnAll operation.
            /// </summary>
            public void ReturnAll(IEnumerable<GameObject> knives)
            {
                foreach (var knife in knives) Return(knife);
            }

            /// <summary>
            /// Performs the CreateKnife operation.
            /// </summary>
            private GameObject CreateKnife()
            {
                var knife = Object.Instantiate(_prefab, _defaultParent);
                _resolver?.InjectGameObject(knife);
                var view = knife.GetComponent<KnifeView>() ?? knife.AddComponent<KnifeView>();
                view.SetColliders(false);
                knife.SetActive(false);
                return knife;
            }
        }
    }
}
