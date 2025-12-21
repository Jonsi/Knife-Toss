using Common.Events;
using Common.MVP.Popup;
using Game.App.Flow.Events;
using Game.Flow.Level;
using Game.Infrastructure.Factories;
using Game.Presentation.Popups.StageFail;
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
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;

        private IPopupNavigationService _NavigationService;
        private StageFailFactorySO _StageFailFactory;

        [Inject]
        private void Construct(
            IPopupNavigationService navigationService,
            StageFailFactorySO stageFailFactory)
        {
            _NavigationService = navigationService;
            _StageFailFactory = stageFailFactory;
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
