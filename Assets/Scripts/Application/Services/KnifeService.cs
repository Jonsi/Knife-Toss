using System.Collections.Generic;
using KnifeToss.Core.Abstractions;
using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Application.Services
{
    public class KnifeService : IKnifeService
    {
        private readonly KnifePool _knifePool;
        private readonly KnifeMagazine _magazine;

        public KnifeService(
            GameObject knifePrefab,
            Transform defaultParent,
            Transform launcherTransform,
            IObjectResolver objectResolver,
            KnifeThrower.MagazineConfig magazineConfig)
        {
            _knifePool = new KnifePool(knifePrefab, defaultParent, magazineConfig.PoolWarmupCount, objectResolver);
            _magazine = new KnifeMagazine(
                _knifePool,
                defaultParent,
                launcherTransform,
                magazineConfig.VisibleQueueCount,
                magazineConfig.QueueSpacing,
                magazineConfig.SlideAnimationDuration,
                magazineConfig.QueueScaleEnd,
                magazineConfig.QueueOpacityEnd);
        }

        public void Configure(int totalKnives) => _magazine.Configure(totalKnives);
        public Knife GetNextKnife() => _magazine.TryTakeNext(out var knifeObject) ? knifeObject.GetComponent<Knife>() : null;
        public void Return(Knife knife) => _knifePool.Return(knife.gameObject);
        public void SlideQueueForward() => _magazine.SlideQueueForward();
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

            public void Configure(int totalKnives)
            {
                Reset();
                KnivesRemaining = totalKnives;
                _pool.Warmup(Mathf.Min(totalKnives, _visibleQueueCount + 2));
                UpdateQueue(true);
            }

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

            public void SlideQueueForward() => UpdateQueue(false);

            public void Reset()
            {
                KillTweens();
                _pool.ReturnAll(_queuedKnives);
                _queuedKnives.Clear();
                KnivesRemaining = 0;
            }

            private void KillTweens()
            {
                foreach (var knife in _queuedKnives)
                {
                    knife.GetComponent<KnifeView>()?.KillTweens();
                }
            }

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

            private void PrepareNewKnife(GameObject knife)
            {
                var view = knife.GetComponent<KnifeView>();
                view.SetParent(_queueParent);
                view.SetColliders(false);
                knife.SetActive(true);
            }

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

            private Vector3 GetQueuePosition(int queueIndex)
            {
                var origin = _launcher != null ? _launcher.position : _queueParent.position;
                var offset = _launcher.right * (-queueIndex * _queueSpacing);
                return origin + offset;
            }
            
            private float GetQueueScaleMultiplier(int queueIndex)
            {
                if (queueIndex == 0) return 1f;
                if (_visibleQueueCount <= 1) return 1f;
                var t = Mathf.Clamp01((float)(queueIndex - 1) / (_visibleQueueCount - 1));
                return Mathf.Lerp(1f, _queueScaleEnd, t);
            }

            private float GetQueueOpacityMultiplier(int queueIndex)
            {
                if (queueIndex > _visibleQueueCount) return 0f;
                if (_visibleQueueCount <= 1 && queueIndex > 0) return 0f;
                if (queueIndex == 0) return 1f;
                var t = Mathf.Clamp01((float)(queueIndex - 1) / (_visibleQueueCount - 1));
                return Mathf.Lerp(1f, _queueOpacityEnd, t);
            }

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

            public void Warmup(int count)
            {
                if (_prefab == null) return;
                while (_pool.Count < count)
                {
                    _pool.Enqueue(CreateKnife());
                }
            }

            public GameObject GetOrCreate()
            {
                if (_prefab == null) return null;
                if (_pool.Count == 0) Warmup(1);
                return _pool.Count > 0 ? _pool.Dequeue() : CreateKnife();
            }

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

            public void ReturnAll(IEnumerable<GameObject> knives)
            {
                foreach (var knife in knives) Return(knife);
            }

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
