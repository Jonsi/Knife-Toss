using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Common.Rewards.Interfaces
{
    /// <summary>
    /// Presents a list of rewards to the player (e.g. popup) and completes when dismissed.
    /// </summary>
    public interface IRewardPresenter
    {
        /// <summary>
        /// Shows rewards with the given title and waits until the player acknowledges.
        /// </summary>
        /// <param name="title">Header text for the presentation.</param>
        /// <param name="rewards">Rewards to display (already granted).</param>
        UniTask PresentAsync(string title, IReadOnlyList<IRewardItem> rewards);
    }
}
