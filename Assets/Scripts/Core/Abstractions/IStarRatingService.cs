using KnifeToss.Core.Domain;

namespace KnifeToss.Core.Abstractions
{
    public interface IStarRatingService
    {
        int CalculateStars(LevelSession session);
    }
}
