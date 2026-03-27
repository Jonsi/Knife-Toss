using Common.Rewards.Events;
using Common.Rewards.Interfaces;
using Game.Persistence.Services;
using Game.Rewards.Presentation;
using Game.Rewards.Services;
using Game.UI.Popups;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Registers reward grant services and the reward presenter for the stage scope.
    /// </summary>
    public class RewardInstaller : IInstaller
    {
        private readonly RewardGrantedEventChannel _GrantedChannel;
        private readonly RewardPopup _Popup;

        /// <summary>
        /// Creates the installer with optional channel and popup references from the scene.
        /// </summary>
        public RewardInstaller(RewardGrantedEventChannel grantedChannel, RewardPopup popup)
        {
            _GrantedChannel = grantedChannel;
            _Popup = popup;
        }

        /// <inheritdoc/>
        public void Install(IContainerBuilder builder)
        {
            builder.Register<RewardClaimTracker>(Lifetime.Singleton);

            if (_Popup != null)
            {
                builder.RegisterComponent(_Popup).As<IRewardPresenter>();
            }
            else
            {
                builder.Register<NullRewardPresenter>(Lifetime.Singleton).As<IRewardPresenter>();
            }

            if (_GrantedChannel != null)
            {
                builder.RegisterInstance(_GrantedChannel).AsSelf();
            }

            builder.Register(CreateRewardGrantService, Lifetime.Singleton).As<IRewardGrantService>();
        }

        private IRewardGrantService CreateRewardGrantService(IObjectResolver resolver)
        {
            return new RewardGrantService(
                resolver.Resolve<IRewardGrantContext>(),
                resolver.Resolve<IRewardPresenter>(),
                _GrantedChannel,
                resolver.Resolve<PlayerDataService>());
        }
    }
}
