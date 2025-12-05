using Core.Domain;

namespace Core.Abstractions
{
    /// <summary>
    /// Defines the IStarRatingService interface.
    /// </summary>
    public interface IStarRatingService
    {
        /// <summary>
        /// Performs the CalculateStars operation.
        /// </summary>
        int CalculateStars(LevelSession session);
    }
}
