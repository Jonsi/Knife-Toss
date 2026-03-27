using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Common.Rewards.Interfaces
{
    /// <summary>
    /// Central entry point for granting rewards, persisting, raising analytics hooks, and optional UI.
    /// </summary>
    public interface IRewardGrantService
    {
        /// <summary>
        /// Grants each reward, saves player data, raises the reward-granted channel,
        /// and optionally shows the reward presenter when <paramref name="presentationTitle"/> is set.
        /// </summary>
        /// <param name="rewards">Rewards to grant in order.</param>
        /// <param name="presentationTitle">If non-null and non-empty, shows the reward UI with this title after granting.</param>
        UniTask GrantAsync(IReadOnlyList<IRewardItem> rewards, string presentationTitle = null);
    }
}
