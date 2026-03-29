using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Missions.Data;
using Game.Missions.Daily.Services;
using Game.Missions.Events;
using UnityEngine;

namespace Game.UI.Missions
{
    /// <summary>
    /// Coordinates the daily mission popup and keeps it synchronized with provider state.
    /// </summary>
    public class DailyMissionPopupController : IDisposable
    {
        private readonly DailyMissionPopup _Popup;
        private readonly DailyMissionProvider _Provider;
        private readonly MissionProgressChangedEventChannel _ProgressChannel;
        private readonly List<MissionItemView> _SpawnedItems = new();
        private readonly HashSet<int> _ClaimsInFlight = new();

        private CancellationTokenSource _RefreshTimerCancellation;
        private bool _IsSubscribedToProgress;

        public DailyMissionPopupController(
            DailyMissionPopup popup,
            DailyMissionProvider provider,
            MissionProgressChangedEventChannel progressChannel)
        {
            _Popup = popup;
            _Provider = provider;
            _ProgressChannel = progressChannel;

            if (_Popup?.CloseButton != null)
            {
                _Popup.CloseButton.onClick.AddListener(HideMissions);
            }
        }

        public void Dispose()
        {
            StopRefreshTimer();
            UnsubscribeFromProgress();

            if (_Popup?.CloseButton != null)
            {
                _Popup.CloseButton.onClick.RemoveListener(HideMissions);
            }

            ClearItems();
        }

        public void ShowMissions()
        {
            if (_Popup == null || _Provider == null)
            {
                return;
            }

            RebuildItems();
            UpdateRefreshTimer();
            SubscribeToProgress();
            _Popup.Show();
            StartRefreshTimer();
        }

        public void HideMissions()
        {
            StopRefreshTimer();
            UnsubscribeFromProgress();
            _Popup?.Hide();
        }

        private void SubscribeToProgress()
        {
            if (_IsSubscribedToProgress || _ProgressChannel == null)
            {
                return;
            }

            _ProgressChannel.Subscribe(OnProgressChanged);
            _IsSubscribedToProgress = true;
        }

        private void UnsubscribeFromProgress()
        {
            if (!_IsSubscribedToProgress || _ProgressChannel == null)
            {
                return;
            }

            _ProgressChannel.Unsubscribe(OnProgressChanged);
            _IsSubscribedToProgress = false;
        }

        private void RebuildItems()
        {
            if (_Popup == null || _Provider == null)
            {
                return;
            }

            if (_Popup.TitleText != null)
            {
                _Popup.TitleText.text = "Daily Missions";
            }

            ClearItems();

            var snapshots = _Provider.GetActiveMissions();
            if (_Popup.MissionItemsRoot == null || _Popup.MissionItemPrefab == null || snapshots == null)
            {
                return;
            }

            for (var i = 0; i < snapshots.Count; i++)
            {
                var itemView = UnityEngine.Object.Instantiate(_Popup.MissionItemPrefab, _Popup.MissionItemsRoot);
                itemView.Setup(i, snapshots[i], OnClaimClicked);
                _SpawnedItems.Add(itemView);
            }
        }

        private void RefreshVisibleItems()
        {
            if (_Provider == null)
            {
                return;
            }

            var snapshots = _Provider.GetActiveMissions();
            if (snapshots == null)
            {
                return;
            }

            if (snapshots.Count != _SpawnedItems.Count)
            {
                RebuildItems();
                return;
            }

            for (var i = 0; i < _SpawnedItems.Count; i++)
            {
                var itemView = _SpawnedItems[i];
                if (itemView != null)
                {
                    itemView.Refresh(snapshots[i]);
                }
            }
        }

        private void RefreshItem(int missionIndex)
        {
            if (_Provider == null || missionIndex < 0 || missionIndex >= _SpawnedItems.Count)
            {
                return;
            }

            var snapshots = _Provider.GetActiveMissions();
            if (snapshots == null || missionIndex >= snapshots.Count)
            {
                RebuildItems();
                return;
            }

            var itemView = _SpawnedItems[missionIndex];
            if (itemView != null)
            {
                itemView.Refresh(snapshots[missionIndex]);
            }
        }

        private void ClearItems()
        {
            if (_Popup?.MissionItemsRoot != null)
            {
                for (var i = _Popup.MissionItemsRoot.childCount - 1; i >= 0; i--)
                {
                    UnityEngine.Object.Destroy(_Popup.MissionItemsRoot.GetChild(i).gameObject);
                }
            }

            _SpawnedItems.Clear();
            _ClaimsInFlight.Clear();
        }

        private void UpdateRefreshTimer()
        {
            if (_Popup?.RefreshTimerText == null || _Provider == null)
            {
                return;
            }

            var remaining = _Provider.GetTimeUntilNextRefreshUtc();
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            _Popup.RefreshTimerText.text = $"Resets in {FormatTimeRemaining(remaining)}";
        }

        private void StartRefreshTimer()
        {
            StopRefreshTimer();
            _RefreshTimerCancellation = new CancellationTokenSource();
            RunRefreshTimerLoopAsync(_RefreshTimerCancellation.Token).Forget();
        }

        private void StopRefreshTimer()
        {
            if (_RefreshTimerCancellation == null)
            {
                return;
            }

            _RefreshTimerCancellation.Cancel();
            _RefreshTimerCancellation.Dispose();
            _RefreshTimerCancellation = null;
        }

        private async UniTaskVoid RunRefreshTimerLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UpdateRefreshTimer();
                    RefreshVisibleItems();
                    await UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnProgressChanged(MissionProgressChangedEventData data)
        {
            if (!ReferenceEquals(data.Source, _Provider))
            {
                return;
            }

            RefreshItem(data.MissionIndex);
        }

        private void OnClaimClicked(int missionIndex)
        {
            TryClaimMissionAsync(missionIndex).Forget();
        }

        private async UniTaskVoid TryClaimMissionAsync(int missionIndex)
        {
            if (_Provider == null || missionIndex < 0 || missionIndex >= _SpawnedItems.Count || !_ClaimsInFlight.Add(missionIndex))
            {
                return;
            }

            var itemView = _SpawnedItems[missionIndex];
            itemView?.SetClaimInteractable(false);

            try
            {
                await _Provider.TryClaimRewardAsync(missionIndex);
                RefreshItem(missionIndex);
            }
            finally
            {
                _ClaimsInFlight.Remove(missionIndex);
            }
        }

        private static string FormatTimeRemaining(TimeSpan remaining)
        {
            if (remaining.TotalHours >= 1d)
            {
                return $"{Mathf.Max(0, (int)remaining.TotalHours)}h {Mathf.Max(0, remaining.Minutes)}m";
            }

            if (remaining.TotalMinutes >= 1d)
            {
                return $"{Mathf.Max(0, (int)remaining.TotalMinutes)}m";
            }

            return $"{Mathf.Max(0, remaining.Seconds)}s";
        }
    }
}
