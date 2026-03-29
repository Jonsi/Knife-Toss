using System.Collections.Generic;
using Game.Missions.Data;
using Game.Missions.Events;
using Game.Missions.Interfaces;
using Game.Persistence.Services;

namespace Game.Missions.Services
{
    /// <summary>
    /// Broadcasts gameplay progress to all registered mission providers and raises shared events.
    /// </summary>
    public class MissionTracker
    {
        private readonly List<IMissionProvider> _Providers = new();
        private readonly MissionProgressChangedEventChannel _ProgressChannel;
        private readonly MissionCompletedEventChannel _CompletedChannel;
        private readonly PlayerDataService _PlayerDataService;

        public MissionTracker(
            MissionProgressChangedEventChannel progressChannel,
            MissionCompletedEventChannel completedChannel,
            PlayerDataService playerDataService)
        {
            _ProgressChannel = progressChannel;
            _CompletedChannel = completedChannel;
            _PlayerDataService = playerDataService;
        }

        public void RegisterProvider(IMissionProvider provider)
        {
            if (provider == null || _Providers.Contains(provider))
            {
                return;
            }

            _Providers.Add(provider);
        }

        public void UnregisterProvider(IMissionProvider provider)
        {
            if (provider == null)
            {
                return;
            }

            _Providers.Remove(provider);
        }

        public void ReportProgress(string typeKey, int amount)
        {
            if (string.IsNullOrEmpty(typeKey) || amount <= 0 || _Providers.Count == 0)
            {
                return;
            }

            var hasChanges = false;
            for (var providerIndex = 0; providerIndex < _Providers.Count; providerIndex++)
            {
                var provider = _Providers[providerIndex];
                if (provider == null || !provider.HasActiveMissions)
                {
                    continue;
                }

                var changes = provider.ReportProgress(typeKey, amount);
                if (changes == null || changes.Count == 0)
                {
                    continue;
                }

                hasChanges = true;
                for (var changeIndex = 0; changeIndex < changes.Count; changeIndex++)
                {
                    var change = changes[changeIndex];
                    _ProgressChannel?.Raise(new MissionProgressChangedEventData(
                        provider,
                        change.MissionIndex,
                        change.CurrentCount,
                        change.RequiredCount));

                    if (change.JustCompleted)
                    {
                        _CompletedChannel?.Raise(new MissionCompletedEventData(
                            provider,
                            change.MissionIndex,
                            change.Definition));
                    }
                }
            }

            if (hasChanges)
            {
                _PlayerDataService.RequestSave();
            }
        }

        public IReadOnlyList<MissionSnapshot> GetAllActiveMissions()
        {
            var snapshots = new List<MissionSnapshot>();
            for (var i = 0; i < _Providers.Count; i++)
            {
                var provider = _Providers[i];
                if (provider == null || !provider.HasActiveMissions)
                {
                    continue;
                }

                snapshots.AddRange(provider.GetActiveMissions());
            }

            return snapshots;
        }
    }
}
