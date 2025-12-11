using System;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Feedback.VFX.Data;
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
        private bool _IsStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="VFXFeedbackService"/> class.
        /// </summary>
        public VFXFeedbackService(
            VFXSettings vfxSettings,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel)
        {
            _VfxSettings = vfxSettings;
            _KnifeImpactChannel = knifeImpactChannel;
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
            var prefab = _VfxSettings?.KnifeHitTargetParticlesPrefab;
            if (prefab == null)
            {
                return;
            }

            UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
        }
    }
}

