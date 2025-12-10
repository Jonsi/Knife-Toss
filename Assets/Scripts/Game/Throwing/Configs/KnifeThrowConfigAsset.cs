using Common.Config;
using Game.Throwing.Data;
using UnityEngine;

namespace Game.Throwing.Configs
{
    /// <summary>
    /// ScriptableObject asset containing configuration data for knife throwing behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeThrowConfig", menuName = "Knife Toss/Gameplay/Knife Throw Config")]
    public class KnifeThrowConfigAsset : ScriptableObject, IConfigurationProvider<KnifeThrowConfiguration>
    {
        /// <summary>
        /// Gets the speed at which knives are thrown.
        /// </summary>
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;
        
        /// <summary>
        /// Gets the spawn position offset relative to the thrower's position.
        /// </summary>
        [field: SerializeField] public Vector2 SpawnPositionOffset { get; private set; } = Vector2.zero;

        /// <summary>
        /// Gets the knife throw configuration.
        /// </summary>
        /// <returns>A KnifeThrowConfiguration object containing the configuration values.</returns>
        public KnifeThrowConfiguration GetConfiguration()
        {
            return new KnifeThrowConfiguration
            {
                ThrowSpeed = ThrowSpeed,
                SpawnPositionOffset = SpawnPositionOffset
            };
        }
    }
}

