using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;

namespace Game.Rewards.Presentation
{
    /// <summary>
    /// No-op presenter used when the reward popup is not configured in the scene.
    /// </summary>
    public class NullRewardPresenter : IRewardPresenter
    {
        /// <inheritdoc/>
        public UniTask PresentAsync(string title, IReadOnlyList<IRewardItem> rewards)
        {
            return UniTask.CompletedTask;
        }
    }
}
