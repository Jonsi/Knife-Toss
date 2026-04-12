using System.Collections.Generic;
using Game.Missions.Interfaces;
using UnityEngine;

namespace Game.Missions.Configs
{
    /// <summary>
    /// Mission pool consumed by a provider.
    /// </summary>
    [CreateAssetMenu(menuName = "Knife Toss/Missions/Mission Catalog")]
    public class MissionCatalog : ScriptableObject, IMissionsProvider
    {
        [field: SerializeField]
        [field: Tooltip("Pool of definitions eligible for this provider.")]
        public List<MissionDefinition> MissionPool { get; private set; } = new();

        [field: SerializeField]
        [field: Min(1)]
        [field: Tooltip("How many missions this provider should keep active at once.")]
        public int ActiveMissionCount { get; private set; } = 3;

        IReadOnlyList<MissionDefinition> IMissionsProvider.MissionPool => MissionPool;
    }
}
