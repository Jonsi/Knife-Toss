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

        public override string Key => "StageLevel";
        public override string Title => "2. Stage/Level";
        public override string IconStyleKey => "Icon_BugReport";
        public override int SortIndex => -109;

        public StageLevelTab(IObjectResolver resolver)
        {
        }

        private StageLevelCheats Cheats => TryResolve<StageLevelCheats>(CurrentResolver);

        [Category("Stage Navigation")]
        [DisplayName("Target Stage (1-based)")]
        public int TargetStage
        {
            get => _TargetStageIndex + 1;
            set
            {
                var resolver = CurrentResolver;
                if (resolver == null) return;
                
                var catalog = TryResolve<ILevelCatalog>(resolver);
                int maxStage = catalog?.StageCount - 1 ?? 999;
                _TargetStageIndex = Mathf.Clamp(value - 1, 0, maxStage);
            }
        }
        
        [Category("Stage Navigation")]
        [DisplayName("Jump To Stage")]
        public void JumpToStage()
        {
            Cheats?.JumpToStage(_TargetStageIndex);
            RequestRefresh();
        }

        [Category("Stage Navigation")]
        [DisplayName("Restart Current Stage")]
        public void RestartStage()
        {
            Cheats?.RestartStage();
        }
        
        [Category("Level Navigation")]
        [DisplayName("Current Level")]
        public int CurrentLevel
        {
            get
            {
                var resolver = CurrentResolver;
                if (resolver == null) return 1;
                var stageData = TryResolve<StageData>(resolver);
                return (stageData?.CurrentLevelIndex ?? 0) + 1;
            }
        }

        [Category("Level Navigation")]
        [DisplayName("Next Level")]
        public void NextLevel()
        {
            Cheats?.NextLevel();
            RequestRefresh();
        }

        [Category("Level Navigation")]
        [DisplayName("Previous Level")]
        public void PreviousLevel()
        {
            Cheats?.PreviousLevel();
            RequestRefresh();
        }

        [Category("Level Navigation")]
        [DisplayName("Restart Current Level")]
        public void RestartLevel()
        {
            Cheats?.RestartLevel();
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
