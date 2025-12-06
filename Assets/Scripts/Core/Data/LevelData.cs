using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Represents the TargetGroupConfig class.
    /// </summary>
    [System.Serializable]
    public class TargetGroupConfig
    {
        [Tooltip("Optional list of targets for this level. If empty, a single default target will be used.")]
        public System.Collections.Generic.List<TargetDefinition> targets = new System.Collections.Generic.List<TargetDefinition>();
    }

    /// <summary>
    /// Represents the LevelData class.
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        [Tooltip("Total number of knives available to the player for this level.")]
        public int knives = 5;

        [Header("Target Group")]
        [Tooltip("Defines the targets in this level and their orbit/scaling settings.")]
        public TargetGroupConfig targetGroup = new TargetGroupConfig();
    }
}
