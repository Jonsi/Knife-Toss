using Core.Abstractions;
using Core.Domain;

namespace App.Services
{
    /// <summary>
    /// Represents the StarRatingService class.
    /// </summary>
    public class StarRatingService : IStarRatingService
    {
        /// <summary>
        /// Performs the CalculateStars operation.
        /// </summary>
        public int CalculateStars(LevelSession session)
        {
            if (session == null || session.Rules.Objectives == null || !session.IsComplete)
            {
                return 0;
            }


            return ObjectiveEvaluator.CalculateStars(
                session.Rules.Objectives,
                new ObjectiveEvaluator.ObjectiveEvaluationContext(
                    session.PlayerKnivesStuck,
                    session.Rules.KnifeCount,
                    session.HitableObjectsHit,
                    session.Rules.TotalHitableObjects));
        }
    }
}
