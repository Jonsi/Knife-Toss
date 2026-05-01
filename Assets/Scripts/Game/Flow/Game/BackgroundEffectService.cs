using Game.Level.Catalogs;
using UnityEngine;

namespace Game.Flow.Game
{
    /// <summary>
    /// Instantiates the main menu background particle for the active world and replaces prior instances.
    /// </summary>
    public class BackgroundEffectService
    {
        private GameObject _ActiveEffectInstance;

        /// <summary>
        /// Destroys any existing effect, then parents a new instance of the collection prefab under the holder.
        /// </summary>
        /// <param name="collection">World whose prefab to spawn; null prefab is ignored.</param>
        /// <param name="holder">Parent transform in the main menu canvas.</param>
        public void ApplyEffect(StagesCollection collection, Transform holder)
        {
            ClearActiveEffect();

            if (collection == null || holder == null)
            {
                return;
            }

            GameObject prefab = collection.MenuParticleEffectPrefab;
            if (prefab == null)
            {
                return;
            }

            _ActiveEffectInstance = Object.Instantiate(prefab, holder);
            _ActiveEffectInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _ActiveEffectInstance.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Removes the spawned effect instance if present.
        /// </summary>
        public void ClearActiveEffect()
        {
            if (_ActiveEffectInstance == null)
            {
                return;
            }

            Object.Destroy(_ActiveEffectInstance);
            _ActiveEffectInstance = null;
        }
    }
}
