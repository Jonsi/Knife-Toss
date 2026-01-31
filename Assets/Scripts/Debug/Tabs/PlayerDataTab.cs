#if TEST_ENABLED
using System.Collections.Generic;
using System.Linq;
using Game.Flow.Game;
using Game.Persistence.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Debug tab displaying player data.
    /// </summary>
    public class PlayerDataTab : DebugTab
    {
        public override string Key => "PlayerData";
        public override string Title => "Player";
        public override int SortIndex => -100;

        public PlayerDataTab()
        {
        }

        public PlayerDataTab(IObjectResolver resolver) : this()
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

        public override IEnumerable<DebugTabSection> GetSections()
        {
            var resolver = CurrentResolver;
            if (resolver == null) yield break;

            var playerDataService = resolver.Resolve<PlayerDataService>();
            var gameData = resolver.Resolve<GameData>();

            yield return new DebugTabSection("Progress")
                .Add("Highest Stage", playerDataService.GetHighestStageIndex())
                .Add("Current Stage", gameData.CurrentStageIndex)
                .Add("Total Collected", gameData.TotalCollected);

            yield return new DebugTabSection("Settings")
                .Add("Is Muted", playerDataService.Settings.IsMuted ? "Yes" : "No");
        }
    }
}
#endif
