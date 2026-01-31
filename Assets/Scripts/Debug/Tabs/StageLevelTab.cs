#if TEST_ENABLED
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Common.Events;
using Game.Flow.Game;
using Game.Flow.Stage;
using Game.Level.Interfaces;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Debugging.Tabs
{
    public class StageLevelTab : OptionsTab
    {
        private int _TargetStageIndex;
        private VoidEventChannel _RetryChannel;
        private VoidEventChannel _RestartChannel;

        public override string Key => "StageLevel";
        public override string Title => "2. Stage/Level";
        public override string IconStyleKey => "Icon_BugReport";
        public override int SortIndex => -109;

        public StageLevelTab(IObjectResolver resolver)
        {
        }

        private VoidEventChannel RetryChannel => _RetryChannel ??= FindChannel("RetryLevelClicked");
        private VoidEventChannel RestartChannel => _RestartChannel ??= FindChannel("RestartStageClicked");

        [Category("Stage Navigation")]
        [DisplayName("Target Stage (0-based)")]
        public int TargetStage
        {
            get => _TargetStageIndex;
            set
            {
                var resolver = CurrentResolver;
                if (resolver == null) return;
                
                var catalog = TryResolve<ILevelCatalog>(resolver);
                int maxStage = catalog?.StageCount - 1 ?? 999;
                _TargetStageIndex = Mathf.Clamp(value, 0, maxStage);
            }
        }
        
        [Category("Stage Navigation")]
        [DisplayName("Jump To Stage")]
        public void JumpToStage()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;
            
            var gameData = TryResolve<GameData>(resolver);
            if (gameData == null)
            {
                Debug.LogWarning("[StageLevelTab] GameData not available - are you in a stage scene?");
                return;
            }
            
            var stageData = TryResolve<StageData>(resolver);
            
            SetInternalProperty(gameData, nameof(GameData.CurrentStageIndex), _TargetStageIndex);
            
            if (stageData != null)
            {
                SetInternalProperty(stageData, nameof(StageData.StageIndex), _TargetStageIndex);
                SetInternalProperty(stageData, nameof(StageData.CurrentLevelIndex), 0);
            }
            
            RestartChannel?.Raise();
            RequestRefresh();
        }

        [Category("Stage Navigation")]
        [DisplayName("Restart Current Stage")]
        public void RestartStage()
        {
            RestartChannel?.Raise();
        }
        
        [Category("Level Navigation")]
        [DisplayName("Current Level")]
        public int CurrentLevel
        {
            get
            {
                var resolver = CurrentResolver;
                if (resolver == null) return 0;
                var stageData = TryResolve<StageData>(resolver);
                return stageData?.CurrentLevelIndex ?? 0;
            }
        }

        [Category("Level Navigation")]
        [DisplayName("Next Level")]
        public void NextLevel()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;
            
            var stageData = TryResolve<StageData>(resolver);
            var catalog = TryResolve<ILevelCatalog>(resolver);
            
            if (stageData == null || catalog == null) return;
            
            int maxLevel = catalog.GetLevelCount(stageData.StageIndex) - 1;
            int nextLevel = Mathf.Min(stageData.CurrentLevelIndex + 1, maxLevel);
            
            SetInternalProperty(stageData, nameof(StageData.CurrentLevelIndex), nextLevel);
            RetryChannel?.Raise();
            RequestRefresh();
        }

        [Category("Level Navigation")]
        [DisplayName("Previous Level")]
        public void PreviousLevel()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;
            
            var stageData = TryResolve<StageData>(resolver);
            if (stageData == null) return;
            
            int prevLevel = Mathf.Max(stageData.CurrentLevelIndex - 1, 0);
            
            SetInternalProperty(stageData, nameof(StageData.CurrentLevelIndex), prevLevel);
            RetryChannel?.Raise();
            RequestRefresh();
        }

        [Category("Level Navigation")]
        [DisplayName("Restart Current Level")]
        public void RestartLevel()
        {
            RetryChannel?.Raise();
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
