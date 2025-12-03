using UnityEngine;

namespace KnifeToss.Core.Data
{
    [System.Serializable]
    public class LevelRequirementsConfig
    {
        [Tooltip("Number of knives the player must stick to complete the level")]
        public int requiredKnives = 5;

        [Tooltip("Maximum number of knives the player can throw (0 = unlimited)")]
        public int maxKnives = 0;
    }

    [System.Serializable]
    public class TargetGroupConfig
    {
        [Tooltip("Optional list of targets for this level. If empty, a single default target will be used.")]
        public System.Collections.Generic.List<TargetDefinition> targets = new System.Collections.Generic.List<TargetDefinition>();
    }

    [System.Serializable]
    public class LevelData
    {
        [Header("Level Requirements")]
        [Tooltip("Knife limits and completion requirement for this level.")]
        public LevelRequirementsConfig requirements = new LevelRequirementsConfig();

        [Header("Target Group")]
        [Tooltip("Defines the targets in this level and their orbit/scaling settings.")]
        public TargetGroupConfig targetGroup = new TargetGroupConfig();
    }
}

