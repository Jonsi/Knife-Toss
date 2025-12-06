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
        [Tooltip("Total number of knives available to the player for this level. Calculated automatically from targets' knivesToBreak if not set.")]
        public int knives = 0;

        [Header("Target Group")]
        [Tooltip("Defines the targets in this level and their orbit/scaling settings.")]
        public TargetGroupConfig targetGroup = new TargetGroupConfig();

        /// <summary>
        /// Calculates the total number of knives needed based on all targets' knivesToBreak values.
        /// </summary>
        public int CalculateKnivesFromTargets()
        {
            if (targetGroup?.targets == null || targetGroup.targets.Count == 0)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < targetGroup.targets.Count; i++)
            {
                TargetDefinition target = targetGroup.targets[i];
                if (target != null)
                {
                    total += Mathf.Max(0, target.knivesToBreak);
                }
            }

            return total;
        }

        /// <summary>
        /// Gets the effective knife count, using calculated value if knives is 0 or less.
        /// </summary>
        public int GetEffectiveKnifeCount()
        {
            int calculated = CalculateKnivesFromTargets();
            if (knives > 0)
            {
                return knives;
            }
            return Mathf.Max(1, calculated);
        }
    }
}
