using KnifeToss.Core.Events;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Spawns splinter particles when knives stick into targets.
    /// </summary>
    public sealed class KnifeImpactParticlesSpawner : MonoBehaviour
    {
        [Header("Event Source")]
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("VFX")]
        [SerializeField] private ParticleSystem splintersPrefab;
        [SerializeField] private Transform vfxParent;

        private bool isSubscribed;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (isSubscribed || impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeStuckAt += HandleKnifeStuckAt;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeStuckAt -= HandleKnifeStuckAt;
            isSubscribed = false;
        }

        private void HandleKnifeStuckAt(Vector3 position)
        {
            SpawnSplinters(position);
        }

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
