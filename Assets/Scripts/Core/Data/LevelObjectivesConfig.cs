using System;
using System.Collections.Generic;

namespace Core.Data
{
    /// <summary>
    /// Types of level objectives.
    /// </summary>
    public enum ObjectiveType
    {
        StickKnives = 0,
        HitHitableObjects = 1
    }

    /// <summary>
    /// Represents a single objective for a star tier.
    /// </summary>
    [Serializable]
    public sealed class ObjectiveDefinition
    {
        public ObjectiveType type = ObjectiveType.StickKnives;

        [UnityEngine.Tooltip("If true, the player must complete the full amount (all knives or all hitables). Otherwise the count field is used.")]
        public bool requireAll = true;

        [UnityEngine.Tooltip("Target count for this objective when RequireAll is false.")]
        public int count = 0;
    }

    /// <summary>
    /// Represents the objectives for a single star tier.
    /// </summary>
    [Serializable]
    public sealed class LevelObjectiveTier
    {
        public List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();
    }

    /// <summary>
    /// Groups objectives for each star rating.
    /// </summary>
    [Serializable]
    public sealed class LevelObjectivesConfig
    {
        [UnityEngine.Tooltip("Objectives required to earn 1 star (also used for level completion).")]
        public LevelObjectiveTier oneStar = new LevelObjectiveTier();

        [UnityEngine.Tooltip("Objectives required to earn 2 stars.")]
        public LevelObjectiveTier twoStar = new LevelObjectiveTier();

        [UnityEngine.Tooltip("Objectives required to earn 3 stars.")]
        public LevelObjectiveTier threeStar = new LevelObjectiveTier();
    }
}
