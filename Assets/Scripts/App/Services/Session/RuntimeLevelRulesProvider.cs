using KnifeToss.Core.Abstractions.Config;
using KnifeToss.Core.Data.Level;

namespace App.Services.Session
{
    /// <summary>
    /// Configuration provider that holds mutable LevelRules set at runtime.
    /// Allows the game to switch rules per level dynamically.
    /// </summary>
    public class RuntimeLevelRulesProvider : IConfigurationProvider<LevelRules>
    {
        private LevelRules _currentRules;

        /// <summary>
        /// Updates the current level rules.
        /// </summary>
        /// <param name="rules">The new rules to apply.</param>
        public void SetRules(LevelRules rules)
        {
            _currentRules = rules;
        }

        /// <summary>
        /// Gets the current level rules configuration.
        /// Returns default rules if none set.
        /// </summary>
        /// <returns>The current LevelRules.</returns>
        public LevelRules GetConfiguration()
        {
            return _currentRules ?? new LevelRules(10, 5);
        }
    }
}
