using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;

namespace KnifeToss.Application.Services
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
            if (!session.IsComplete)
            {
                return 0;
            }


            var stars = 1;
            var isCleanRun = !session.HadToRetry;
            
            var allBonusObjectsHit = false; 

            if (isCleanRun || allBonusObjectsHit)
            {
                stars = 2;
            }

            if (isCleanRun && allBonusObjectsHit)
            {
                stars = 3;
            }

            return stars;
        }
    }
}
