using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;

namespace KnifeToss.Application.Services
{
    public class StarRatingService : IStarRatingService
    {
        public int CalculateStars(LevelSession session)
        {
            if (!session.IsComplete)
            {
                return 0;
            }

            // GDD criteria:
            // 1 Star: Level completed.
            // 2 Stars: "Clean Run" (no failures) OR all bonus objects hit.
            // 3 Stars: "Perfect Run" (no failures AND all bonus objects hit).

            var stars = 1;
            var isCleanRun = !session.HadToRetry;
            
            // TODO: Implement bonus object tracking
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
