#if TEST_ENABLED
using System.Linq;
using System.Reflection;
using Common.Events;
using Game.Flow.Game;
using Game.Flow.Stage;
using Game.Level.Interfaces;
using UnityEngine;

namespace Game.Debugging
{
    public class StageLevelCheats
    {
        private readonly GameData _gameData;
        private readonly StageData _stageData;
        private readonly ILevelCatalog _levelCatalog;
        
        private VoidEventChannel _retryChannel;
        private VoidEventChannel _restartChannel;

        public StageLevelCheats(
            GameData gameData,
            StageData stageData,
            ILevelCatalog levelCatalog)
        {
            _gameData = gameData;
            _stageData = stageData;
            _levelCatalog = levelCatalog;
        }

        private VoidEventChannel RetryChannel => _retryChannel ??= FindChannel("RetryLevelClicked");
        private VoidEventChannel RestartChannel => _restartChannel ??= FindChannel("RestartStageClicked");

        public void RestartLevel()
        {
            RetryChannel?.Raise();
        }

        public void RestartStage()
        {
            RestartChannel?.Raise();
        }

        public void JumpToStage(int stageIndex)
        {
            if (_gameData == null) return;
            
            int maxStage = _levelCatalog?.StageCount - 1 ?? 999;
            int targetIndex = Mathf.Clamp(stageIndex, 0, maxStage);
            
            SetInternalProperty(_gameData, nameof(GameData.CurrentStageIndex), targetIndex);
            
            if (_stageData != null)
            {
                SetInternalProperty(_stageData, nameof(StageData.StageIndex), targetIndex);
                SetInternalProperty(_stageData, nameof(StageData.CurrentLevelIndex), 0);
            }
            
            RestartStage();
        }

        public void NextLevel()
        {
            if (_stageData == null || _levelCatalog == null) return;
            
            int maxLevel = _levelCatalog.GetLevelCount(_stageData.StageIndex) - 1;
            int nextLevel = Mathf.Min(_stageData.CurrentLevelIndex + 1, maxLevel);
            
            SetInternalProperty(_stageData, nameof(StageData.CurrentLevelIndex), nextLevel);
            RestartLevel();
        }

        public void PreviousLevel()
        {
            if (_stageData == null) return;
            
            int prevLevel = Mathf.Max(_stageData.CurrentLevelIndex - 1, 0);
            
            SetInternalProperty(_stageData, nameof(StageData.CurrentLevelIndex), prevLevel);
            RestartLevel();
        }

        private void SetInternalProperty<T>(object target, string propName, T value)
        {
            var prop = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(target, value);
        }

        private VoidEventChannel FindChannel(string assetName)
        {
            return Resources.FindObjectsOfTypeAll<VoidEventChannel>()
                .FirstOrDefault(c => c.name == assetName);
        }
    }
}
#endif
