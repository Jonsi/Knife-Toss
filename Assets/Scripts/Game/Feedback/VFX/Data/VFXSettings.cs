using UnityEngine;

namespace Game.Feedback.VFX.Data
{
    /// <summary>
    /// Stores particle effect prefabs used for gameplay feedback.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXSettings", menuName = "Knife Toss/VFX/VFX Settings")]
    public class VFXSettings : ScriptableObject
    {
        [field: SerializeField] public GameObject KnifeHitTargetParticlesPrefab { get; private set; }
        [field: SerializeField] public GameObject CollectibleHitParticlesPrefab { get; private set; }
    }
}

