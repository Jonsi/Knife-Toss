using System.Collections.Generic;
using Common.Rewards.Events;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Persistence.Services;

namespace Game.Rewards.Services
{
    /// <summary>
    /// Grants rewards, persists player data, raises the reward-granted channel, and optionally shows UI.
    /// </summary>
    public class RewardGrantService : IRewardGrantService
    {
        private readonly IRewardGrantContext _GrantContext;
        private readonly IRewardPresenter _Presenter;
        private readonly RewardGrantedEventChannel _GrantedChannel;
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Creates the grant service with its dependencies.
        /// </summary>
        public RewardGrantService(
            IRewardGrantContext grantContext,
            IRewardPresenter presenter,
            RewardGrantedEventChannel grantedChannel,
            PlayerDataService playerDataService)
        {
            _GrantContext = grantContext;
            _Presenter = presenter;
            _GrantedChannel = grantedChannel;
            _PlayerDataService = playerDataService;
        }

        /// <inheritdoc/>
        public async UniTask GrantAsync(IReadOnlyList<IRewardItem> rewards, string presentationTitle = null)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return;
            }

            var validRewards = new List<IRewardItem>(rewards.Count);
            var displayTexts = new List<string>();

            foreach (var reward in rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                await reward.GrantAsync(_GrantContext);
                validRewards.Add(reward);
                var text = reward.DisplayText;
                if (!string.IsNullOrEmpty(text))
                {
                    displayTexts.Add(text);
                }
            }

            if (_GrantedChannel != null)
            {
                _GrantedChannel.Raise(new RewardGrantedEventData(displayTexts));
            }

            _PlayerDataService.ForceSave();

            if (!string.IsNullOrEmpty(presentationTitle) && validRewards.Count > 0 && _Presenter != null)
            {
                await _Presenter.PresentAsync(presentationTitle, validRewards);
            }
        }
    }
}
