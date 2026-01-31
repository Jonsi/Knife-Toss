#if TEST_ENABLED
using System.ComponentModel;
using System.Linq;
using Common.Events;
using Game.Flow.Game;
using Game.Persistence.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Debugging.Tabs
{
    public class GeneralTab : OptionsTab
    {
        public override string Key => "General";
        public override string Title => "1. General";
        public override string IconStyleKey => "Icon_Settings";
        public override int SortIndex => -110;

        public GeneralTab(IObjectResolver resolver)
        {
        }

        private static IObjectResolver CurrentResolver
        {
            get
            {
                var scopes = Object.FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None);
                foreach (var scope in scopes)
                {
                    if (scope.gameObject.scene.name != "DontDestroyOnLoad" && scope.Container != null)
                        return scope.Container;
                }
                return scopes.FirstOrDefault(s => s.Container != null)?.Container;
            }
        }

        private static T TryResolve<T>(IObjectResolver resolver) where T : class
        {
            return resolver?.Resolve<T>();
        }

        [Category("Progress")]
        [DisplayName("Highest Stage")]
        public int HighestStage
        {
            get
            {
                var resolver = CurrentResolver;
                return TryResolve<PlayerDataService>(resolver)?.GetHighestStageIndex() ?? 0;
            }
        }

        [Category("Progress")]
        [DisplayName("Current Stage")]
        public int CurrentStage
        {
            get
            {
                var resolver = CurrentResolver;
                return TryResolve<GameData>(resolver)?.CurrentStageIndex ?? 0;
            }
        }

        [Category("Progress")]
        [DisplayName("Total Collected")]
        public int TotalCollected
        {
            get
            {
                var resolver = CurrentResolver;
                return TryResolve<GameData>(resolver)?.TotalCollected ?? 0;
            }
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
            RequestRefresh();
        }
    }
}
#endif