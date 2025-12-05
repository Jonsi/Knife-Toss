using System;
using Core.Data;

namespace Core.Domain
{
    /// <summary>
    /// Evaluates authored objectives against runtime progress.
    /// </summary>
    public static class ObjectiveEvaluator
    {
        public readonly struct ObjectiveEvaluationContext
        {
            public ObjectiveEvaluationContext(int playerKnivesStuck, int knifeTargetCount, int hitableObjectsHit, int totalHitableObjects)
            {
                PlayerKnivesStuck = Math.Max(0, playerKnivesStuck);
                KnifeTargetCount = Math.Max(0, knifeTargetCount);
                HitableObjectsHit = Math.Max(0, hitableObjectsHit);
                TotalHitableObjects = Math.Max(0, totalHitableObjects);
            }

            public int PlayerKnivesStuck { get; }
            public int KnifeTargetCount { get; }
            public int HitableObjectsHit { get; }
            public int TotalHitableObjects { get; }
        }

        /// <summary>
        /// Calculates how many stars have been earned based on the objectives and progress.
        /// </summary>
        public static int CalculateStars(LevelObjectivesConfig objectives, ObjectiveEvaluationContext context)
        {
            if (objectives == null)
            {
                return 0;
            }

            int stars = 0;
            if (HasObjectives(objectives.oneStar))
            {
                if (AreObjectivesMet(objectives.oneStar, context))
                {
                    stars = 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

            if (stars == 1 && HasObjectives(objectives.twoStar) && AreObjectivesMet(objectives.twoStar, context))
            {
                stars = 2;
            }

            if (stars == 2 && HasObjectives(objectives.threeStar) && AreObjectivesMet(objectives.threeStar, context))
            {
                stars = 3;
            }

            return stars;
        }

        /// <summary>
        /// Checks whether a tier of objectives is complete.
        /// </summary>
        public static bool AreObjectivesMet(LevelObjectiveTier tier, ObjectiveEvaluationContext context)
        {
            if (tier?.objectives == null || tier.objectives.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < tier.objectives.Count; i++)
            {
                ObjectiveDefinition objective = tier.objectives[i];
                if (objective == null)
                {
                    continue;
                }

                if (!IsObjectiveMet(objective, context))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasObjectives(LevelObjectiveTier tier)
        {
            return tier != null && tier.objectives != null && tier.objectives.Count > 0;
        }

        private static bool IsObjectiveMet(ObjectiveDefinition objective, ObjectiveEvaluationContext context)
        {
            switch (objective.type)
            {
                case ObjectiveType.StickKnives:
                    return HasReachedTarget(context.PlayerKnivesStuck, objective.requireAll ? context.KnifeTargetCount : objective.count);
                case ObjectiveType.HitHitableObjects:
                    return HasReachedTarget(context.HitableObjectsHit, objective.requireAll ? context.TotalHitableObjects : objective.count);
                default:
                    return false;
            }
        }

        private static bool HasReachedTarget(int current, int target)
        {
            if (target <= 0)
            {
                return true;
            }

            return current >= target;
        }
    }
}
