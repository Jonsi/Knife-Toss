using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// ScriptableObject asset containing configuration data for target behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "TargetConfig", menuName = "Knife Toss/Gameplay/Target Config")]
    public class TargetConfigAsset : ScriptableObject, IConfigurationProvider<TargetConfiguration>
    {
        /// <summary>
        /// Gets the rotation speed in degrees per second.
        /// </summary>
        [field: SerializeField] public float RotationSpeed { get; private set; } = 50f;

        /// <summary>
        /// Gets the radius of the target.
        /// </summary>
        [field: SerializeField] public float Radius { get; private set; } = 1f;

        /// <summary>
        /// Gets the target configuration.
        /// </summary>
        /// <returns>A TargetConfiguration object containing the configuration values.</returns>
        public TargetConfiguration GetConfiguration()
        {
            return new TargetConfiguration
            {
                RotationSpeed = RotationSpeed,
                Radius = Radius
            };
        }
    }
}

