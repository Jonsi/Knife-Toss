using System.Collections.Generic;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Creates, resets, and recycles knife instances.
    /// </summary>
    internal sealed class KnifePool
    {
        private readonly GameObject prefab;
        private readonly Transform defaultParent;
        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        public KnifePool(GameObject knifePrefab, Transform parent, int warmupCount)
        {
            prefab = knifePrefab;
            defaultParent = parent;

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

        public void Return(GameObject knife)
        {
            if (knife == null)
            {
                return;
            }

            TweenKiller.Kill(knife);

            Transform knifeTransform = knife.transform;
            if (knifeTransform != null)
            {
                knifeTransform.SetParent(defaultParent);
                knifeTransform.localScale = PrefabOriginalScale;
            }

            SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = PrefabOriginalColor;
            }

            DisableColliders(knife);
            knife.SetActive(false);
            pool.Enqueue(knife);
        }

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

        public void Clear()
        {
            pool.Clear();
        }

        public void KillTweens()
        {
            TweenKiller.Kill(pool);
        }

        private GameObject CreateKnife()
        {
            GameObject knife = Object.Instantiate(prefab, defaultParent);
            knife.SetActive(false);
            DisableColliders(knife);
            return knife;
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
