using System.Collections.Generic;
using KnifeToss.Core.Abstractions;
using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Application.Services
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
            KnifeToss.Core.Data.MagazineConfig magazineConfig)
        {
            if (magazineConfig == null)
            {
                // Default values if config not provided
                _knifePool = new KnifePool(knifePrefab, defaultParent, 10, objectResolver);
                _magazine = new KnifeMagazine(_knifePool, defaultParent, launcherTransform, 5, 0.5f, 0.3f, 0.5f, 0.3f);
            }
            else
            {
                _knifePool = new KnifePool(knifePrefab, defaultParent, magazineConfig.poolWarmupCount, objectResolver);
                _magazine = new KnifeMagazine(
                    _knifePool,
                    defaultParent,
                    launcherTransform,
                    magazineConfig.visibleQueueCount,
                    magazineConfig.queueSpacing,
                    magazineConfig.slideAnimationDuration,
                    magazineConfig.queueScaleEnd,
                    magazineConfig.queueOpacityEnd);
            }
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
        public void SlideQueueForward() => _magazine.SlideQueueForward();
        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset() => _magazine.Reset();

        private sealed class KnifeMagazine
        {
            private readonly KnifePool _pool;
            private readonly Transform _queueParent;
            private readonly Transform _launcher;
            private readonly int _visibleQueueCount;
            private readonly float _queueSpacing;
            private readonly float _slideAnimationDuration;
            private readonly float _queueScaleEnd;
            private readonly float _queueOpacityEnd;
            private readonly List<GameObject> _queuedKnives = new List<GameObject>();
            public int KnivesRemaining { get; private set; }

            /// <summary>
            /// Performs the KnifeMagazine operation.
            /// </summary>
            public KnifeMagazine(KnifePool knifePool, Transform queueRoot, Transform launcherTransform, int visibleCount, float spacing, float slideDuration, float scaleEnd, float opacityEnd)
            {
                _pool = knifePool;
                _queueParent = queueRoot;
                _launcher = launcherTransform;
                _visibleQueueCount = Mathf.Max(1, visibleCount);
                _queueSpacing = spacing;
                _slideAnimationDuration = slideDuration;
                _queueScaleEnd = scaleEnd;
                _queueOpacityEnd = opacityEnd;
            }

            /// <summary>
            /// Performs the Configure operation.
            /// </summary>
            public void Configure(int totalKnives)
            {
                Reset();
                KnivesRemaining = totalKnives;
                _pool.Warmup(Mathf.Min(totalKnives, _visibleQueueCount + 2));
                UpdateQueue(true);
            }

            /// <summary>
            /// Performs the TryTakeNext operation.
            /// </summary>
            public bool TryTakeNext(out GameObject knife)
            {
                knife = null;
                if (KnivesRemaining <= 0 || _queuedKnives.Count == 0) return false;

                var knifeView = _queuedKnives[0].GetComponent<KnifeView>();
                knifeView.KillTweens();
                knife = _queuedKnives[0];
                _queuedKnives.RemoveAt(0);
                KnivesRemaining--;
                return knife != null;
            }

            /// <summary>
            /// Performs the SlideQueueForward operation.
            /// </summary>
            public void SlideQueueForward() => UpdateQueue(false);

            /// <summary>
            /// Performs the Reset operation.
            /// </summary>
            public void Reset()
            {
                KillTweens();
                _pool.ReturnAll(_queuedKnives);
                _queuedKnives.Clear();
                KnivesRemaining = 0;
            }

            /// <summary>
            /// Performs the KillTweens operation.
            /// </summary>
            private void KillTweens()
            {
                foreach (var knife in _queuedKnives)
                {
                    knife.GetComponent<KnifeView>()?.KillTweens();
                }
            }

            /// <summary>
            /// Performs the UpdateQueue operation.
            /// </summary>
            private void UpdateQueue(bool instantaneous)
            {
                for (var i = 0; i < _queuedKnives.Count; i++)
                {
                    ApplyKnifeVisuals(_queuedKnives[i], i, instantaneous);
                }

                var desiredQueueSize = Mathf.Min(KnivesRemaining, _visibleQueueCount + 1);
                while (_queuedKnives.Count < desiredQueueSize)
                {
                    var knife = _pool.GetOrCreate();
                    if (knife == null) break;

                    var newKnifeIndex = _queuedKnives.Count;
                    _queuedKnives.Add(knife);
                    PrepareNewKnife(knife);
                    ApplyKnifeVisuals(knife, newKnifeIndex, instantaneous);
                }
            }

            /// <summary>
            /// Performs the PrepareNewKnife operation.
            /// </summary>
            private void PrepareNewKnife(GameObject knife)
            {
                var view = knife.GetComponent<KnifeView>();
                view.SetParent(_queueParent);
                view.SetColliders(false);
                knife.SetActive(true);
            }

            /// <summary>
            /// Performs the ApplyKnifeVisuals operation.
            /// </summary>
            private void ApplyKnifeVisuals(GameObject knife, int index, bool instantaneous)
            {
                var view = knife.GetComponent<KnifeView>();
                if (view == null) return;
                
                view.KillTweens();
                
                var targetPosition = GetQueuePosition(index);
                var targetScale = _pool.PrefabOriginalScale * GetQueueScaleMultiplier(index);
                var targetColor = GetQueueColor(index);

                if (instantaneous)
                {
                    view.ApplyVisuals(targetPosition, targetScale, targetColor, _launcher.rotation);
                }
                else
                {
                    view.AnimateTo(targetPosition, targetScale, targetColor, _slideAnimationDuration);
                }
            }

            /// <summary>
            /// Performs the GetQueuePosition operation.
            /// </summary>
            private Vector3 GetQueuePosition(int queueIndex)
            {
                var origin = _launcher != null ? _launcher.position : _queueParent.position;
                var offset = _launcher.right * (-queueIndex * _queueSpacing);
                return origin + offset;
            }
            
            /// <summary>
            /// Performs the GetQueueScaleMultiplier operation.
            /// </summary>
            private float GetQueueScaleMultiplier(int queueIndex)
            {
                if (queueIndex == 0) return 1f;
                if (_visibleQueueCount <= 1) return 1f;
                var t = Mathf.Clamp01((float)(queueIndex - 1) / (_visibleQueueCount - 1));
                return Mathf.Lerp(1f, _queueScaleEnd, t);
            }

            /// <summary>
            /// Performs the GetQueueOpacityMultiplier operation.
            /// </summary>
            private float GetQueueOpacityMultiplier(int queueIndex)
            {
                if (queueIndex > _visibleQueueCount) return 0f;
                if (_visibleQueueCount <= 1 && queueIndex > 0) return 0f;
                if (queueIndex == 0) return 1f;
                var t = Mathf.Clamp01((float)(queueIndex - 1) / (_visibleQueueCount - 1));
                return Mathf.Lerp(1f, _queueOpacityEnd, t);
            }

            /// <summary>
            /// Performs the GetQueueColor operation.
            /// </summary>
            private Color GetQueueColor(int index)
            {
                var opacity = GetQueueOpacityMultiplier(index);
                return _pool.PrefabOriginalColor * new Color(1f, 1f, 1f, opacity);
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
