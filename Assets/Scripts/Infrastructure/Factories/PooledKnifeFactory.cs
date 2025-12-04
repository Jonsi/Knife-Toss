using KnifeToss.Core.Abstractions.Factories;
using KnifeToss.Infrastructure.Pooling;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Infrastructure.Factories
{
    /// <summary>
    /// Factory for creating knives using a pooled system.
    /// </summary>
    public sealed class PooledKnifeFactory : IKnifeFactory
    {
        private readonly ComponentPool<Knife> _pool;
        private readonly IObjectResolver _resolver;

        /// <summary>
        /// Creates a new pooled knife factory.
        /// </summary>
        public PooledKnifeFactory(GameObject prefab, Transform parent, IObjectResolver resolver, int warmupCount = 15)
        {
            _resolver = resolver;
            _pool = new ComponentPool<Knife>(
                factory: () => CreateKnife(prefab, parent),
                parent: parent,
                onGet: OnKnifeRetrieved,
                onRelease: OnKnifeReleased
            );
            _pool.Warmup(warmupCount);
        }

        public Knife Create()
        {
            var knife = _pool.Get();
            knife.OnSpawn();
            return knife;
        }

        public void Recycle(Knife knife)
        {
            if (knife == null)
            {
                return;
            }

            knife.OnDespawn();
            _pool.Release(knife);
        }

        private Knife CreateKnife(GameObject prefab, Transform parent)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.SetActive(false);
            _resolver.InjectGameObject(instance);

            Knife knife = instance.GetComponent<Knife>();
            if (knife == null)
            {
                Debug.LogError($"Knife prefab '{prefab.name}' does not have a Knife component!");
            }

            return knife;
        }

        private void OnKnifeRetrieved(Knife knife)
        {
            // Additional setup when knife is retrieved from pool
        }

        private void OnKnifeReleased(Knife knife)
        {
            // Additional cleanup when knife is returned to pool
        }
    }
}

