using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Defines a hitable object's visuals and feedback.
    /// </summary>
    [CreateAssetMenu(fileName = "HitableObjectConfig", menuName = "Knife Toss/Configurations/Gameplay/Hitable Object Config")]
    public sealed class HitableObjectConfig : ScriptableObject
    {
        [Tooltip("Prefab to spawn for this hitable object.")]
        public GameObject prefab;

        [Tooltip("Particles spawned when this object is hit.")]
        public ParticleSystem hitParticles;
    }
}
