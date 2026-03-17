using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.Flow.Game;
using Game.PVP.Input;
using Game.PVP.Services;
using Game.PVP.UI;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using Game.Entities.Target.Factories;
using Game.Entities.Target.Data;
using Game.PVP.Configs;

namespace Game.PVP.Flow
{
    public class PVPFlowController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private KnifeView _KnifePrefab;
        
        private PVPInputController _InputController;
        private PVPThrowerManager _ThrowerManager;
        private PVPHudController _HudController;
        private IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private SceneLoaderService _SceneLoaderService;
        private TargetFactory _TargetFactory;
        private PVPSettingsConfig _PvpConfig;
        
        private bool _IsPlayer1Turn = true;
        private bool _IsGameActive = false;
        private bool _PendingBounceFailure;

        [Inject]
        public void Construct(
            PVPInputController inputController,
            PVPThrowerManager throwerManager,
            PVPHudController hudController,
            IEventChannel<KnifeImpactEventData> impactChannel,
            SceneLoaderService sceneLoaderService,
            TargetFactory targetFactory,
            PVPSettingsConfig pvpConfig)
        {
            _InputController = inputController;
            _ThrowerManager = throwerManager;
            _HudController = hudController;
            _ImpactChannel = impactChannel;
            _SceneLoaderService = sceneLoaderService;
            _TargetFactory = targetFactory;
            _PvpConfig = pvpConfig;
        }

        private void Start()
        {
            _InputController.Initialize();
            _InputController.OnThrowRequested += HandleThrow;
            _ImpactChannel.Subscribe(HandleImpact);
            
            _HudController.OnRematchClicked += ReloadScene;
            _HudController.OnExitClicked += ExitGame;

            StartGame();
        }

        private void OnDestroy()
        {
            if (_InputController != null) _InputController.OnThrowRequested -= HandleThrow;
            if (_ImpactChannel != null) _ImpactChannel.Unsubscribe(HandleImpact);
            if (_HudController != null)
            {
                 _HudController.OnRematchClicked -= ReloadScene;
                 _HudController.OnExitClicked -= ExitGame;
            }
        }

        private void StartGame()
        {
            _IsGameActive = true;
            _IsPlayer1Turn = true;
            _PendingBounceFailure = false;
            _HudController.HideWinnerOverlay();
            _HudController.UpdateTurnIndicator(_IsPlayer1Turn);
            
            SpawnTarget();
            LoadKnife();
            _InputController.SetInputLocked(false);
        }
        
        private void ReloadScene()
        {
             _SceneLoaderService.LoadPVP().Forget();
        }

        private void SpawnTarget()
        {
            var config = _PvpConfig.TargetConfig;
            
            var targetView = _TargetFactory.SpawnTarget(config, null);
            
            // Set sorting order to ensure visibility above background (similar to LevelSpawner)
            // Using index 1 gives BaseSortingOrder = 10, Knives = 9 (if offset -1)
            targetView.SetSortingOrder(1);

            _TargetFactory.AttachElements(targetView, config, null);
            _TargetFactory.FinalizeTarget(targetView);
        }

        private void HandleThrow()
        {
            if (!_IsGameActive) return;

            _ThrowerManager.ThrowFromPlayer(_IsPlayer1Turn);
            _InputController.SetInputLocked(true);
        }

        private void HandleImpact(KnifeImpactEventData data)
        {
            if (!_IsGameActive) return;

            if (data.ImpactType == KnifeImpactType.Stuck)
            {
                // Successful hit
                SwitchTurn();
            }
            else if (data.ImpactType == KnifeImpactType.Bounced)
            {
                _PendingBounceFailure |= data.CountsAsFailure;
            }
            else if (data.ImpactType == KnifeImpactType.OutOfView && _PendingBounceFailure)
            {
                _PendingBounceFailure = false;
                EndGame(!_IsPlayer1Turn);
            }
        }

        private void SwitchTurn()
        {
            _IsPlayer1Turn = !_IsPlayer1Turn;
            _HudController.UpdateTurnIndicator(_IsPlayer1Turn);
            LoadKnife();
            _InputController.SetInputLocked(false);
        }

        private void LoadKnife()
        {
            _ThrowerManager.LoadKnifeForPlayer(_IsPlayer1Turn, _KnifePrefab);
        }

        private void EndGame(bool player1Wins)
        {
            _IsGameActive = false;
            _HudController.ShowWinner(player1Wins);
        }

        private void ExitGame()
        {
            _SceneLoaderService.LoadMainMenu().Forget();
        }
    }
}
