using Common.Events;
using Common.MVP.Core;
using Game.App.Flow.Events;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using R3;
using VContainer.Unity;

namespace Game.Presentation.HUD.PowerUps
{
    /// <summary>
    /// Presenter that coordinates power-up HUD state.
    /// </summary>
    public class PowerUpHudPresenter : BasePresenter<PowerUpHudView, PowerUpHudModel>, IStartable
    {
        private readonly IEventChannel<PowerUpActivatedEventData> _ActivatedChannel;
        private readonly IEventChannel<PowerUpUsedEventData> _UsedChannel;
        private readonly IEventChannel<LevelStartedEventData> _LevelStartedChannel;
        private readonly IPowerUpService _PowerUpService;
        private readonly PlayerDataService _PlayerDataService;

        public PowerUpHudPresenter(
            PowerUpHudView view,
            IEventChannel<PowerUpActivatedEventData> activatedChannel,
            IEventChannel<PowerUpUsedEventData> usedChannel,
            IEventChannel<LevelStartedEventData> levelStartedChannel,
            IPowerUpService powerUpService,
            PlayerDataService playerDataService)
            : base(view, new PowerUpHudModel())
        {
            _ActivatedChannel = activatedChannel;
            _UsedChannel = usedChannel;
            _LevelStartedChannel = levelStartedChannel;
            _PowerUpService = powerUpService;
            _PlayerDataService = playerDataService;
        }

        public void Start()
        {
            View.Initialize(Model);
            Model.SuperKnifeCount
                .Subscribe(_ => ApplyCounts())
                .AddTo(disposables);
            Model.MultipleKnivesCount
                .Subscribe(_ => ApplyCounts())
                .AddTo(disposables);
            Model.CollectiblesBladeCount
                .Subscribe(_ => ApplyCounts())
                .AddTo(disposables);
            Model.SelectedConfig
                .Subscribe(config => View.SetSelectedPowerUp(config))
                .AddTo(disposables);

            View.ButtonClicked
                .Subscribe(OnButtonClicked)
                .AddTo(disposables);

            _ActivatedChannel
                ?.AsObservable()
                .Subscribe(OnActivated)
                .AddTo(disposables);

            _UsedChannel
                ?.AsObservable()
                .Subscribe(_ => OnUsed())
                .AddTo(disposables);

            _LevelStartedChannel
                ?.AsObservable()
                .Subscribe(_ => OnLevelStarted())
                .AddTo(disposables);

            RefreshInventoryAndCounts();
            ClearSelection();
        }

        private void OnButtonClicked(PowerUpConfigBase config)
        {
            if (_PowerUpService == null || config == null)
            {
                return;
            }

            if (_PowerUpService.TryActivate(config))
            {
                Model.SelectedConfig.Value = config;
            }
        }

        private void OnActivated(PowerUpActivatedEventData data)
        {
            if (data == null)
            {
                return;
            }

            if (data.Config != null)
            {
                Model.SelectedConfig.Value = data.Config;
            }

            RefreshCounts();
        }

        private void OnUsed()
        {
            RefreshCounts();
            ClearSelection();
        }

        private void OnLevelStarted()
        {
            ClearSelection();
            RefreshInventoryAndCounts();
        }

        private void ClearSelection()
        {
            Model.SelectedConfig.Value = null;
        }

        private void RefreshInventoryAndCounts()
        {
            _PlayerDataService?.ReloadPowerUpInventory();
            RefreshCounts();
        }

        private void RefreshCounts()
        {
            if (_PlayerDataService == null)
            {
                return;
            }

            var inventory = _PlayerDataService.EnsurePowerUpInventory();
            ApplyInventory(inventory);
        }

        private void ApplyInventory(PowerUpInventoryData inventory)
        {
            if (inventory == null)
            {
                Model.SuperKnifeCount.Value = 0;
                Model.MultipleKnivesCount.Value = 0;
                Model.CollectiblesBladeCount.Value = 0;
                return;
            }

            Model.SuperKnifeCount.Value = inventory.SuperKnifeCount;
            Model.MultipleKnivesCount.Value = inventory.MultipleKnivesCount;
            Model.CollectiblesBladeCount.Value = inventory.CollectiblesBladeCount;
        }

        private void ApplyCounts()
        {
            View.SetPowerUpCounts(
                Model.SuperKnifeCount.Value,
                Model.MultipleKnivesCount.Value,
                Model.CollectiblesBladeCount.Value);
        }
    }
}
