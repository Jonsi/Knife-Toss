using System;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Feedback.VFX.Data;
using Game.Pooling;
using UnityEngine;
using VContainer.Unity;

namespace Game.Feedback.VFX.Services
{
    /// <summary>
    /// Spawns visual effects in response to gameplay events.
    /// </summary>
    public class VFXFeedbackService : IStartable, IDisposable
    {
        private readonly VFXSettings _VfxSettings;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private readonly ParticlePoolProvider _KnifeHitPool;
        private bool _IsStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="VFXFeedbackService"/> class.
        /// </summary>
        public VFXFeedbackService(
            VFXSettings vfxSettings,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel,
            ParticlePoolProvider knifeHitPoolProvider)
        {
            _VfxSettings = vfxSettings;
            _KnifeImpactChannel = knifeImpactChannel;
            _KnifeHitPool = knifeHitPoolProvider ?? (vfxSettings?.KnifeHitTargetParticlesPrefab != null
                ? new ParticlePoolProvider(vfxSettings.KnifeHitTargetParticlesPrefab)
                : null);
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_IsStarted)
            {
                return;
            }

            _KnifeImpactChannel?.Subscribe(OnKnifeImpact);
            _IsStarted = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_IsStarted)
            {
                return;
            }

            _KnifeImpactChannel?.Unsubscribe(OnKnifeImpact);
            _IsStarted = false;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (data.ImpactType != KnifeImpactType.Stuck)
            {
                return;
            }

            SpawnKnifeHitParticles(data.ImpactPosition);
        }

        private void SpawnKnifeHitParticles(Vector2 position)
        {
            _KnifeHitPool?.Get(position);
        }
    }
}

