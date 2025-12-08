using Core.Data;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// ScriptableObject asset containing configuration data for knife throwing behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeThrowConfig", menuName = "Knife Toss/Gameplay/Knife Throw Config")]
    public class KnifeThrowConfigAsset : ScriptableObject
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
        /// Converts this ScriptableObject configuration to a pure C# data structure.
        /// </summary>
        /// <returns>A KnifeThrowConfiguration struct containing the configuration values.</returns>
        public KnifeThrowConfiguration ToConfiguration()
        {
            return new KnifeThrowConfiguration
            {
                ThrowSpeed = ThrowSpeed,
                SpawnPositionOffsetX = SpawnPositionOffset.x,
                SpawnPositionOffsetY = SpawnPositionOffset.y
            };
        }
    }
}

