using Game.DI;
using Game.Feedback.VFX.Services;
using Game.Pooling;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs visual feedback systems.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXInstaller", menuName = "Knife Toss/Installers/VFX")]
    public class VFXInstallerSO : BaseInstallerSO
    {
        [Header("Prefabs / Pooling")]
        [SerializeField] private PrefabsConfiguration _PrefabsConfiguration;

        /// <summary>
        /// Registers visual feedback services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            // Only register ParticlePoolProvider if the prefab entry is available
            if (_PrefabsConfiguration?.GetKnifeHitParticles() != null)
            {
                builder.Register<ParticlePoolProvider>(Lifetime.Singleton)
                    .WithParameter(_PrefabsConfiguration)
                    .WithParameter(ParticlePrefabType.KnifeHitTarget);
            }

            builder.Register<VFXFeedbackService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}

