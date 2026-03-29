using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Missions.Data;

namespace Game.Missions.Interfaces
{
    /// <summary>
    /// Contract implemented by any mission source that plugs into the shared tracker.
    /// </summary>
    public interface IMissionProvider
    {
        IReadOnlyList<MissionProgressChange> ReportProgress(string typeKey, int amount);
        IReadOnlyList<MissionSnapshot> GetActiveMissions();
        UniTask<bool> TryClaimRewardAsync(int missionIndex);
        bool HasActiveMissions { get; }
    }
}
