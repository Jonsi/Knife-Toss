using System.Collections.Generic;
using Presentation.Gameplay.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Presentation.Gameplay
{



    internal sealed class KnifePool
    {
        private readonly GameObject prefab;
        private readonly Transform defaultParent;
        private readonly Queue<GameObject> pool = new Queue<GameObject>();
        private readonly IObjectResolver resolver;

        /// <summary>
        /// Performs the KnifePool operation.
        /// </summary>
        public KnifePool(GameObject knifePrefab, Transform parent, int warmupCount, IObjectResolver objectResolver)
        {
            prefab = knifePrefab;
            defaultParent = parent;
            resolver = objectResolver;

            if (prefab != null)
            {
                PrefabOriginalScale = prefab.transform.localScale;
                PrefabOriginalColor = prefab.GetComponent<SpriteRenderer>()?.color ?? Color.white;
            }
            else
            {
                PrefabOriginalScale = Vector3.one;
                PrefabOriginalColor = Color.white;
            }

            Warmup(warmupCount);
        }

        public Vector3 PrefabOriginalScale { get; }
        public Color PrefabOriginalColor { get; }

        /// <summary>
        /// Performs the Warmup operation.
        /// </summary>
        public void Warmup(int count)
        {
            if (prefab == null)
            {
                return;
            }

            while (pool.Count < count)
            {
                pool.Enqueue(CreateKnife());
            }
        }

        /// <summary>
        /// Performs the GetOrCreate operation.
        /// </summary>
        public GameObject GetOrCreate()
        {
            if (prefab == null)
            {
                return null;
            }

            if (pool.Count == 0)
            {
                Warmup(1);
            }

            return pool.Count > 0 ? pool.Dequeue() : CreateKnife();
        }

        /// <summary>
        /// Performs the Return operation.
        /// </summary>
        public void Return(GameObject knife)
        {
            if (knife == null)
            {
                return;
            }

            TweenKiller.Kill(knife);
            ResetKnifeTransform(knife);
            ResetKnifeVisuals(knife);
            DisableColliders(knife);
            knife.SetActive(false);
            pool.Enqueue(knife);
        }

        /// <summary>
        /// Performs the ReturnAll operation.
        /// </summary>
        public void ReturnAll(IEnumerable<GameObject> knives)
        {
            if (knives == null)
            {
                return;
            }

            foreach (GameObject knife in knives)
            {
                Return(knife);
            }
        }

        /// <summary>
        /// Performs the Clear operation.
        /// </summary>
        public void Clear()
        {
            pool.Clear();
        }

        /// <summary>
        /// Performs the KillTweens operation.
        /// </summary>
        public void KillTweens()
        {
            TweenKiller.Kill(pool);
        }

        /// <summary>
        /// Performs the CreateKnife operation.
        /// </summary>
        private GameObject CreateKnife()
        {
            GameObject knife = Object.Instantiate(prefab, defaultParent);
            resolver?.InjectGameObject(knife);
            knife.SetActive(false);
            EnsureView(knife);
            DisableColliders(knife);
            return knife;
        }

        /// <summary>
        /// Performs the DisableColliders operation.
        /// </summary>
        private static void DisableColliders(GameObject knife)
        {
            if (knife == null)
            {
                return;
            }

            KnifeView view = knife.GetComponent<KnifeView>();
            view?.CacheIfNeeded();
            if (view != null)
            {
                view.SetCollidersEnabled(false);
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

        /// <summary>
        /// Performs the ResetKnifeTransform operation.
        /// </summary>
        private void ResetKnifeTransform(GameObject knife)
        {
            Transform knifeTransform = knife.transform;
            if (knifeTransform == null)
            {
                return;
            }

            knifeTransform.SetParent(defaultParent);
            knifeTransform.localScale = PrefabOriginalScale;
        }

        /// <summary>
        /// Performs the ResetKnifeVisuals operation.
        /// </summary>
        private void ResetKnifeVisuals(GameObject knife)
        {
            KnifeView view = EnsureView(knife);
            if (view != null)
            {
                view.SetColor(PrefabOriginalColor);
            }
        }

        /// <summary>
        /// Performs the EnsureView operation.
        /// </summary>
        private static KnifeView EnsureView(GameObject knife)
        {
            if (knife == null)
            {
                return null;
            }

            KnifeView view = knife.GetComponent<KnifeView>() ?? knife.AddComponent<KnifeView>();
            view.CacheIfNeeded();
            return view;
        }
    }
}
