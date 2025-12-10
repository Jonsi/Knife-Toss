using System.Linq;
using KnifeToss.Game.Level.Data;

namespace KnifeToss.Game.Level.States
{
    /// <summary>
    /// Level is initializing and not yet playable.
    /// Handles spawning targets and setting up level rules.
    /// </summary>
    public class LevelLoadingState : LevelStateBase 
    {
        public LevelLoadingState(StageStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            Context.ResetLevelState();

            LevelConfig config = Context.LevelCatalog.GetLevelConfig(Context.CurrentStageIndex, Context.CurrentLevelIndex);
            
            int totalKnivesToBreak = config.Targets.Sum(t => t.KnivesToBreak);
            Context.KnifeCount = config.KnifeCount > 0 ? config.KnifeCount : totalKnivesToBreak + config.ExtraKnives;

            var spawnedTargets = Context.LevelSpawner.SpawnLevel(config);
            Context.ActiveTargets.AddRange(spawnedTargets);

            machine.SetState(machine.playing);
        }
    }
}
