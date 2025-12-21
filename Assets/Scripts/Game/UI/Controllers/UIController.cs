using Common.Events;
using Common.MVP.Popup;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.UI.Factories;
using Game.UI.Popups;
using UnityEngine;
using VContainer;

namespace Game.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Header("Popup Root")]
        [SerializeField] private Transform _PopupRoot;

        [Header("Channels")]
        [SerializeField] private LevelFailedEventChannel _LevelFailedChannel;

        private IPopupNavigationService _NavigationService;
        private StageFailFactorySO _StageFailFactory;
        private VoidEventChannel _RetryLevelChannel;
        private VoidEventChannel _RestartStageChannel;

        [Inject]
        private void Construct(
            IPopupNavigationService navigationService,
            StageFailFactorySO stageFailFactory,
            VoidEventChannel retryLevelChannel,
            VoidEventChannel restartStageChannel)
        {
            _NavigationService = navigationService;
            _StageFailFactory = stageFailFactory;
            _RetryLevelChannel = retryLevelChannel;
            _RestartStageChannel = restartStageChannel;
        }

        private void OnEnable()
        {
            if (_LevelFailedChannel != null)
                _LevelFailedChannel.Subscribe(OnLevelFailed);
        }

        private void OnDisable()
        {
            if (_LevelFailedChannel != null)
                _LevelFailedChannel.Unsubscribe(OnLevelFailed);
        }

        private void OnLevelFailed(LevelData data)
        {
            if (_StageFailFactory == null || _PopupRoot == null || _NavigationService == null)
                return;

            var model = new StageFailPopupModel(
                data,
                () => _RetryLevelChannel?.Raise(),
                () => _RestartStageChannel?.Raise(),
                () => _NavigationService?.CloseTop());
            _NavigationService.OpenPopup(_StageFailFactory, model, _PopupRoot);
        }
    }
}
