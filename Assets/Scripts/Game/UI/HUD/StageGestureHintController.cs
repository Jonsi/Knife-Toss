using Game.Flow.Events;
using Game.Flow.Game;
using Game.KnifeThrower.Events;
using Game.Persistence.Services;
using Game.PowerUps.Data;
using Game.PowerUps.Events;
using Game.UI.Common;
using Game.UI.Data;
using UnityEngine;
using VContainer.Unity;

namespace Game.UI.HUD
{
    /// <summary>
    /// Shows one-time gesture hints for the opening stage tutorial.
    /// </summary>
    public class StageGestureHintController : IInitializable, System.IDisposable
    {
        private const int TutorialCollectionIndex = 0;
        private const int TutorialStageIndex = 0;
        private const int TapToThrowLevelIndex = 0;
        private const int SuperKnifeLevelIndex = 1;
        private const int SlowTimeLevelIndex = 2;
        private const string SuperKnifeInventoryKey = "SuperKnife";
        private const string SlowTimeInventoryKey = "TimeDilator";

        private enum HintType
        {
            None,
            TapToThrow,
            SuperKnife,
            SlowTime
        }

        private readonly GameData _GameData;
        private readonly PlayerDataService _PlayerDataService;
        private readonly KnifeThrownEventChannel _KnifeThrownChannel;
        private readonly LevelStartedEventChannel _LevelStartedChannel;
        private readonly PowerUpActivatedEventChannel _PowerUpActivatedChannel;
        private readonly PowerUpUsedEventChannel _PowerUpUsedChannel;
        private readonly GestureHintConfiguration _GestureHintConfiguration;
        private readonly GameObject _GesturePopupPrefab;
        private readonly Transform _GesturePopupParent;

        private GesturePopupView _GesturePopup;
        private HintType _ActiveHint;

        public StageGestureHintController(
            GameData gameData,
            PlayerDataService playerDataService,
            KnifeThrownEventChannel knifeThrownChannel,
            LevelStartedEventChannel levelStartedChannel,
            PowerUpActivatedEventChannel powerUpActivatedChannel,
            PowerUpUsedEventChannel powerUpUsedChannel,
            GestureHintConfiguration gestureHintConfiguration,
            GameObject gesturePopupPrefab,
            Transform gesturePopupParent)
        {
            _GameData = gameData;
            _PlayerDataService = playerDataService;
            _KnifeThrownChannel = knifeThrownChannel;
            _LevelStartedChannel = levelStartedChannel;
            _PowerUpActivatedChannel = powerUpActivatedChannel;
            _PowerUpUsedChannel = powerUpUsedChannel;
            _GestureHintConfiguration = gestureHintConfiguration;
            _GesturePopupPrefab = gesturePopupPrefab;
            _GesturePopupParent = gesturePopupParent;
        }

        public void Initialize()
        {
            if (_GesturePopupPrefab == null || _GestureHintConfiguration == null)
            {
                return;
            }

            _KnifeThrownChannel?.Subscribe(OnKnifeThrown);
            _LevelStartedChannel?.Subscribe(OnLevelStarted);
            _PowerUpActivatedChannel?.Subscribe(OnPowerUpActivated);
            _PowerUpUsedChannel?.Subscribe(OnPowerUpUsed);

            if (_LevelStartedChannel != null && _LevelStartedChannel.HasLastEvent)
            {
                OnLevelStarted(_LevelStartedChannel.LastEvent);
            }
        }

        public void Dispose()
        {
            _KnifeThrownChannel?.Unsubscribe(OnKnifeThrown);
            _LevelStartedChannel?.Unsubscribe(OnLevelStarted);
            _PowerUpActivatedChannel?.Unsubscribe(OnPowerUpActivated);
            _PowerUpUsedChannel?.Unsubscribe(OnPowerUpUsed);

            if (_GesturePopup != null)
            {
                Object.Destroy(_GesturePopup.gameObject);
                _GesturePopup = null;
            }
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            _GesturePopup?.Hide();
            _ActiveHint = HintType.None;

            if (!IsTutorialStage(data))
            {
                return;
            }

            var onboarding = _PlayerDataService?.GetData()?.Onboarding;
            if (onboarding == null)
            {
                return;
            }

            switch (data.LevelIndex)
            {
                case TapToThrowLevelIndex when !onboarding.HasTappedThrowGesture:
                    ShowHint(HintType.TapToThrow, _GestureHintConfiguration.TapToThrowPosition);
                    break;
                case SuperKnifeLevelIndex when !onboarding.HasTappedSuperKnifeGesture:
                    ShowHint(HintType.SuperKnife, _GestureHintConfiguration.TapSuperKnifePowerUpPosition);
                    break;
                case SlowTimeLevelIndex when !onboarding.HasTappedSlowTimeGesture:
                    ShowHint(HintType.SlowTime, _GestureHintConfiguration.TapSlowTimePowerUpPosition);
                    break;
            }
        }

        private bool IsTutorialStage(LevelStartedEventData data)
        {
            return data != null &&
                _GameData != null &&
                _GameData.CurrentCollectionIndex == TutorialCollectionIndex &&
                data.StageIndex == TutorialStageIndex;
        }

        private void ShowHint(HintType hintType, Vector2 position)
        {
            GesturePopupView gesturePopup = GetOrCreateGesturePopup();
            if (gesturePopup == null)
            {
                return;
            }

            _ActiveHint = hintType;
            gesturePopup.ShowAt(position, false);
        }

        private GesturePopupView GetOrCreateGesturePopup()
        {
            if (_GesturePopup != null)
            {
                return _GesturePopup;
            }

            Transform parent = _GesturePopupParent != null ? _GesturePopupParent : null;
            GameObject instance = Object.Instantiate(_GesturePopupPrefab, parent);

            if (instance.transform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
            }

            _GesturePopup = instance.GetComponent<GesturePopupView>();
            if (_GesturePopup == null)
            {
                _GesturePopup = instance.AddComponent<GesturePopupView>();
            }

            return _GesturePopup;
        }

        private void OnKnifeThrown()
        {
            if (_ActiveHint != HintType.TapToThrow)
            {
                return;
            }

            var data = _PlayerDataService?.GetData();
            if (data?.Onboarding != null)
            {
                data.Onboarding.HasTappedThrowGesture = true;
                _PlayerDataService.ForceSave();
            }

            _GesturePopup?.Hide();
            _ActiveHint = HintType.None;
        }

        private void OnPowerUpActivated(PowerUpActivatedEventData data)
        {
            if (data?.Config == null)
            {
                return;
            }

            if (_ActiveHint == HintType.SuperKnife && data.Config.InventoryKey == SuperKnifeInventoryKey)
            {
                MarkPowerUpGestureTapped(HintType.SuperKnife);
                return;
            }

            if (_ActiveHint == HintType.SlowTime && data.Config.InventoryKey == SlowTimeInventoryKey)
            {
                MarkPowerUpGestureTapped(HintType.SlowTime);
            }
        }

        private void OnPowerUpUsed(PowerUpUsedEventData data)
        {
            if (data?.Config == null)
            {
                return;
            }

            if (_ActiveHint == HintType.SlowTime && data.Config.InventoryKey == SlowTimeInventoryKey)
            {
                MarkPowerUpGestureTapped(HintType.SlowTime);
            }
        }

        private void MarkPowerUpGestureTapped(HintType hintType)
        {
            var data = _PlayerDataService?.GetData();
            if (data?.Onboarding != null)
            {
                if (hintType == HintType.SuperKnife)
                {
                    data.Onboarding.HasTappedSuperKnifeGesture = true;
                }
                else if (hintType == HintType.SlowTime)
                {
                    data.Onboarding.HasTappedSlowTimeGesture = true;
                }

                _PlayerDataService.ForceSave();
            }

            _GesturePopup?.Hide();
            _ActiveHint = HintType.None;
        }

    }
}
