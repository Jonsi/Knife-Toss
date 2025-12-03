using KnifeToss.Core.Events;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{



    public sealed class KnifeImpactParticlesSpawner : MonoBehaviour
    {
        [Header("Event Source")]
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("VFX")]
        [SerializeField] private ParticleSystem splintersPrefab;
        [SerializeField] private Transform vfxParent;

        private bool isSubscribed;

        /// <summary>
        /// Performs the OnEnable operation.
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Performs the Subscribe operation.
        /// </summary>
        private void Subscribe()
        {
            if (isSubscribed || impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeStuckAt += HandleKnifeStuckAt;
            isSubscribed = true;
        }

        /// <summary>
        /// Performs the Unsubscribe operation.
        /// </summary>
        private void Unsubscribe()
        {
            if (!isSubscribed || impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeStuckAt -= HandleKnifeStuckAt;
            isSubscribed = false;
        }

        /// <summary>
        /// Performs the HandleKnifeStuckAt operation.
        /// </summary>
        private void HandleKnifeStuckAt(Vector3 position)
        {
            SpawnSplinters(position);
        }

        /// <summary>
        /// Performs the SpawnSplinters operation.
        /// </summary>
        private void SpawnSplinters(Vector3 position)
        {
            if (splintersPrefab == null)
            {
                return;
            }

            ParticleSystem instance = Instantiate(
                splintersPrefab,
                position,
                Quaternion.identity,
                vfxParent);

            ParticleSystem.MainModule main = instance.main;
            if (!main.loop)
            {
                float life = main.startLifetime.constantMax;
                Destroy(instance.gameObject, main.duration + life);
            }
        }
    }
}
