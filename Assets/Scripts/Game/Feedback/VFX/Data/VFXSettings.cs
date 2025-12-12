using UnityEngine;

namespace Game.Feedback.VFX.Data
{
    /// <summary>
    /// Stores particle effect prefabs used for gameplay feedback.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXSettings", menuName = "Knife Toss/VFX/VFX Settings")]
    public class VFXSettings : ScriptableObject
    {
        [field: SerializeField] public ParticleSystem KnifeHitTargetParticlesPrefab { get; private set; }
        [field: SerializeField] public ParticleSystem CollectibleHitParticlesPrefab { get; private set; }
    }
}

