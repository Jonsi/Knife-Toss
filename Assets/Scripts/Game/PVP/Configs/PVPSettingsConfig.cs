using Game.Entities.Target.Data;
using UnityEngine;

namespace Game.PVP.Configs
{
    [CreateAssetMenu(fileName = "PVPSettingsConfig", menuName = "KnifeToss/PVP/Settings Config")]
    public class PVPSettingsConfig : ScriptableObject
    {
        [Header("Target Settings")]
        [Tooltip("Configuration for the PVP target.")]
        public TargetConfig TargetConfig;
        
        [Header("Throw Settings")]
        public float ThrowSpeed = 20f;
    }
}
