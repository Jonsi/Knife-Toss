using System;
using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Missions.Configs;
using Game.Missions.Data;
using Game.Missions.Daily.Data;
using Game.Missions.Interfaces;
using Game.Missions.Services;
using Game.Persistence.Services;
using Game.Rewards;
using UnityEngine;
using VContainer.Unity;

namespace Game.Missions.Daily.Services
{
    /// <summary>
    /// Daily mission provider with UTC refresh, weighted selection, persistence, and reward claiming.
    /// </summary>
    public class DailyMissionProvider : IMissionProvider, IInitializable, IDisposable
    {
        private readonly IMissionsProvider _MissionsProvider;
        private readonly MissionTracker _Tracker;
        private readonly PlayerDataService _PlayerDataService;
        private readonly IRewardGrantService _RewardGrantService;
        private readonly Dictionary<string, MissionDefinition> _DefinitionsByKey = new(StringComparer.Ordinal);

        private DailyMissionData _Data;

        public DailyMissionProvider(
            IMissionsProvider missionsProvider,
            MissionTracker tracker,
            PlayerDataService playerDataService,
            IRewardGrantService rewardGrantService)
        {
            _MissionsProvider = missionsProvider;
            _Tracker = tracker;
            _PlayerDataService = playerDataService;
            _RewardGrantService = rewardGrantService;
        }

        public bool HasActiveMissions
        {
            get
            {
                EnsureFreshData();
                return _Data != null && _Data.ActiveMissions != null && _Data.ActiveMissions.Count > 0;
            }
        }

        public void Initialize()
        {
            RebuildDefinitionLookup();
            _Data = _PlayerDataService.EnsureDailyMissionData();
            EnsureFreshData();
            _Tracker.RegisterProvider(this);
        }

        public void Dispose()
        {
            _Tracker.UnregisterProvider(this);
        }

        public IReadOnlyList<MissionProgressChange> ReportProgress(string typeKey, int amount)
        {
            EnsureFreshData();

            if (string.IsNullOrEmpty(typeKey) || amount <= 0 || _Data?.ActiveMissions == null)
            {
                return Array.Empty<MissionProgressChange>();
            }

            var changes = new List<MissionProgressChange>();
            for (var missionIndex = 0; missionIndex < _Data.ActiveMissions.Count; missionIndex++)
            {
                var entry = _Data.ActiveMissions[missionIndex];
                if (entry == null || entry.Claimed || !TryResolveDefinition(entry.DefinitionKey, out var definition))
                {
                    continue;
                }

                if (!string.Equals(definition.ObjectiveTypeKey, typeKey, StringComparison.Ordinal))
                {
                    continue;
                }

                var requiredCount = Mathf.Max(1, definition.TargetCount);
                if (entry.CurrentCount >= requiredCount)
                {
                    continue;
                }

                var previousCount = entry.CurrentCount;
                entry.CurrentCount = Mathf.Min(requiredCount, previousCount + amount);
                changes.Add(new MissionProgressChange(
                    missionIndex,
                    definition,
                    entry.CurrentCount,
                    requiredCount,
                    previousCount < requiredCount && entry.CurrentCount >= requiredCount));
            }

            return changes;
        }

        public IReadOnlyList<MissionSnapshot> GetActiveMissions()
        {
            EnsureFreshData();

            if (_Data?.ActiveMissions == null || _Data.ActiveMissions.Count == 0)
            {
                return Array.Empty<MissionSnapshot>();
            }

            var snapshots = new List<MissionSnapshot>(_Data.ActiveMissions.Count);
            for (var i = 0; i < _Data.ActiveMissions.Count; i++)
            {
                snapshots.Add(BuildSnapshot(_Data.ActiveMissions[i]));
            }

            return snapshots;
        }

        public async UniTask<bool> TryClaimRewardAsync(int missionIndex)
        {
            EnsureFreshData();

            if (_Data?.ActiveMissions == null || missionIndex < 0 || missionIndex >= _Data.ActiveMissions.Count)
            {
                return false;
            }

            var entry = _Data.ActiveMissions[missionIndex];
            if (entry == null || entry.Claimed || !TryResolveDefinition(entry.DefinitionKey, out var definition))
            {
                return false;
            }

            var requiredCount = Mathf.Max(1, definition.TargetCount);
            if (entry.CurrentCount < requiredCount)
            {
                return false;
            }

            var rewards = BuildRewardItems(definition);
            entry.Claimed = true;

            if (rewards.Count > 0)
            {
                await _RewardGrantService.GrantAsync(rewards, RewardPresentationTitles.DailyMissionComplete);
            }

            _PlayerDataService.ForceSave();
            return true;
        }

        public TimeSpan GetTimeUntilNextRefreshUtc()
        {
            var now = DateTime.UtcNow;
            var nextRefresh = now.Date.AddDays(1);
            return nextRefresh - now;
        }

        private void EnsureFreshData()
        {
            _Data ??= _PlayerDataService.EnsureDailyMissionData();
            _Data.ActiveMissions ??= new List<ActiveMissionEntry>();

            if (!NeedsRefresh(_Data))
            {
                return;
            }

            RefreshMissions();
        }

        private bool NeedsRefresh(DailyMissionData data)
        {
            if (data == null)
            {
                return true;
            }

            if (!string.Equals(data.RefreshDateUtc, GetTodayUtcKey(), StringComparison.Ordinal))
            {
                return true;
            }

            if (data.ActiveMissions == null)
            {
                return true;
            }

            var expectedCount = GetTargetActiveMissionCount();
            if (data.ActiveMissions.Count != expectedCount)
            {
                return true;
            }

            for (var i = 0; i < data.ActiveMissions.Count; i++)
            {
                var entry = data.ActiveMissions[i];
                if (entry == null || !TryResolveDefinition(entry.DefinitionKey, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshMissions()
        {
            var selectedDefinitions = SelectDefinitions();
            _Data.RefreshDateUtc = GetTodayUtcKey();
            _Data.ActiveMissions.Clear();

            for (var i = 0; i < selectedDefinitions.Count; i++)
            {
                var definition = selectedDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                _Data.ActiveMissions.Add(new ActiveMissionEntry
                {
                    DefinitionKey = definition.DefinitionKey,
                    CurrentCount = 0,
                    Claimed = false
                });
            }

            _PlayerDataService.ForceSave();
        }

        private void RebuildDefinitionLookup()
        {
            _DefinitionsByKey.Clear();

            var pool = _MissionsProvider?.MissionPool;
            if (pool == null)
            {
                return;
            }

            for (var i = 0; i < pool.Count; i++)
            {
                var definition = pool[i];
                if (definition == null)
                {
                    continue;
                }

                var key = definition.DefinitionKey;
                if (string.IsNullOrEmpty(key) || _DefinitionsByKey.ContainsKey(key))
                {
                    continue;
                }

                _DefinitionsByKey.Add(key, definition);
            }
        }

        private List<MissionDefinition> SelectDefinitions()
        {
            var available = new List<MissionDefinition>();
            var pool = _MissionsProvider?.MissionPool;
            if (pool != null)
            {
                for (var i = 0; i < pool.Count; i++)
                {
                    var definition = pool[i];
                    if (definition == null || definition.SelectionWeight <= 0)
                    {
                        continue;
                    }

                    available.Add(definition);
                }
            }

            var selected = new List<MissionDefinition>();
            var targetCount = Mathf.Min(GetTargetActiveMissionCount(), available.Count);
            while (selected.Count < targetCount && available.Count > 0)
            {
                var pickedIndex = SelectWeightedIndex(available);
                if (pickedIndex < 0 || pickedIndex >= available.Count)
                {
                    break;
                }

                selected.Add(available[pickedIndex]);
                available.RemoveAt(pickedIndex);
            }

            return selected;
        }

        private int SelectWeightedIndex(List<MissionDefinition> definitions)
        {
            var totalWeight = 0;
            for (var i = 0; i < definitions.Count; i++)
            {
                totalWeight += Mathf.Max(0, definitions[i].SelectionWeight);
            }

            if (totalWeight <= 0)
            {
                return -1;
            }

            var roll = UnityEngine.Random.Range(0, totalWeight);
            for (var i = 0; i < definitions.Count; i++)
            {
                roll -= Mathf.Max(0, definitions[i].SelectionWeight);
                if (roll < 0)
                {
                    return i;
                }
            }

            return definitions.Count - 1;
        }

        private int GetTargetActiveMissionCount()
        {
            return Mathf.Max(0, _MissionsProvider != null ? _MissionsProvider.ActiveMissionCount : 0);
        }

        private bool TryResolveDefinition(string definitionKey, out MissionDefinition definition)
        {
            if (string.IsNullOrEmpty(definitionKey))
            {
                definition = null;
                return false;
            }

            return _DefinitionsByKey.TryGetValue(definitionKey, out definition) && definition != null;
        }

        private MissionSnapshot BuildSnapshot(ActiveMissionEntry entry)
        {
            if (entry == null || !TryResolveDefinition(entry.DefinitionKey, out var definition))
            {
                return default;
            }

            var requiredCount = Mathf.Max(1, definition.TargetCount);
            var currentCount = Mathf.Clamp(entry.CurrentCount, 0, requiredCount);
            var isCompleted = currentCount >= requiredCount;
            return new MissionSnapshot(definition, currentCount, requiredCount, entry.Claimed, isCompleted);
        }

        private List<IRewardItem> BuildRewardItems(MissionDefinition definition)
        {
            var items = new List<IRewardItem>();
            var rewards = definition?.Rewards;
            if (rewards == null)
            {
                return items;
            }

            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                if (reward != null && reward.TryCreateGrantItem(out var item))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private static string GetTodayUtcKey()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }
    }
}
