#if TEST_ENABLED
using System.Collections.Generic;
using Game.Flow.Game;
using Game.Persistence.Services;
using VContainer;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Debug tab displaying player data.
    /// </summary>
    public class PlayerDataTab : DebugTab
    {
        private readonly PlayerDataService _PlayerDataService;
        private readonly GameData _GameData;

        public override string Key => "PlayerData";
        public override string Title => "Player";
        public override int SortIndex => -100;

        public PlayerDataTab(IObjectResolver resolver)
        {
            _PlayerDataService = resolver.Resolve<PlayerDataService>();
            _GameData = resolver.Resolve<GameData>();
        }

        public override IEnumerable<DebugTabSection> GetSections()
        {
            yield return new DebugTabSection("Progress")
                .Add("Highest Stage", _PlayerDataService.GetHighestStageIndex())
                .Add("Current Stage", _GameData.CurrentStageIndex)
                .Add("Total Collected", _GameData.TotalCollected);

            yield return new DebugTabSection("Settings")
                .Add("Is Muted", _PlayerDataService.Settings.IsMuted ? "Yes" : "No");
        }
    }
}
#endif
