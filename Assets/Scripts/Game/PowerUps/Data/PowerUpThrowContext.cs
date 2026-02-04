using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Interfaces;
using Game.Pooling;
using UnityEngine;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Encapsulates data needed to execute a power-up during a throw request.
    /// </summary>
    public class PowerUpThrowContext
    {
        public PowerUpThrowContext(
            IProjectileThrower throwerView,
            KnifePoolProvider poolProvider,
            KnifeThrowConfiguration throwConfiguration,
            Transform spawnPoint)
        {
            ThrowerView = throwerView;
            PoolProvider = poolProvider;
            ThrowConfiguration = throwConfiguration;
            SpawnPoint = spawnPoint;
        }

        /// <summary>
        /// Gets the thrower view used to launch knives.
        /// </summary>
        public IProjectileThrower ThrowerView { get; }
        /// <summary>
        /// Gets the knife pool provider.
        /// </summary>
        public KnifePoolProvider PoolProvider { get; }
        /// <summary>
        /// Gets the throw configuration for the current level.
        /// </summary>
        public KnifeThrowConfiguration ThrowConfiguration { get; }
        /// <summary>
        /// Gets the spawn transform for creating knives.
        /// </summary>
        public Transform SpawnPoint { get; }
    }
}
