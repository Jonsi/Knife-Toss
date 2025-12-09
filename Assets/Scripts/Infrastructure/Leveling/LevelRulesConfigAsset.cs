using KnifeToss.Core.Abstractions.Config;
using KnifeToss.Core.Data.Level;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// ScriptableObject provider for level rules (knife count and stick threshold).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelRulesConfig", menuName = "Knife Toss/Gameplay/Level Rules Config")]
    public class LevelRulesConfigAsset : ScriptableObject, IConfigurationProvider<LevelRules>
    {
        [field: SerializeField, Min(1)] public int KnifeCount { get; private set; } = 10;
        [field: SerializeField, Min(1)] public int StickThreshold { get; private set; } = 8;

        public LevelRules GetConfiguration()
        {
            return new LevelRules(KnifeCount, StickThreshold);
        }
    }
}

