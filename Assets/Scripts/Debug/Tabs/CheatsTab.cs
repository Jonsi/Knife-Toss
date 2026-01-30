#if TEST_ENABLED
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Common.Events;
using Game.Flow.Game;
using Game.Flow.Stage;
using Game.Level.Interfaces;
using Game.Persistence.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Cheats tab - same code pattern as the original CheatOptions partial class.
    /// Uses [Category] and [DisplayName] attributes.
    /// </summary>
    public class CheatsTab : OptionsTab
    {
        private int _TargetStageIndex;
        private VoidEventChannel _RetryChannel;
        private VoidEventChannel _RestartChannel;

        public override string Key => "Cheats";
        public override string Title => "Cheats";
        public override string IconStyleKey => "Icon_BugReport";
        public override int SortIndex => -110;

        public CheatsTab(IObjectResolver resolver)
        {
        }

        private static IObjectResolver CurrentResolver
        {
            get
            {
                var scope = Object.FindAnyObjectByType<LifetimeScope>();
                return scope?.Container;
            }
        }

        private static T TryResolve<T>(IObjectResolver resolver) where T : class
        {
            try
            {
                return resolver?.Resolve<T>();
            }
            catch
            {
                return null;
            }
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
                Debug.LogWarning("[Cheats] GameData not available - are you in a stage scene?");
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
        
        [Category("Stage Navigation")]
        [DisplayName("Restart Current Stage")]
        public void RestartStage()
        {
            RestartChannel?.Raise();
        }
        
        [Category("Resources")]
        [DisplayName("Add 100 Tomatoes")]
        public void AddTomatoes()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;
            
            var gameData = TryResolve<GameData>(resolver);
            var playerDataService = TryResolve<PlayerDataService>(resolver);
            
            if (gameData == null) return;
            
            int newValue = gameData.TotalCollected + 100;
            SetInternalProperty(gameData, nameof(GameData.TotalCollected), newValue);
            
            playerDataService?.RequestSave();
        }
        
        [Category("Progress")]
        [DisplayName("Clear All Progress")]
        public void ClearProgress()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;
            
            var gameData = TryResolve<GameData>(resolver);
            var playerDataService = TryResolve<PlayerDataService>(resolver);
            
            if (gameData == null) return;
            gameData.Reset();
            playerDataService?.ForceSave();
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
