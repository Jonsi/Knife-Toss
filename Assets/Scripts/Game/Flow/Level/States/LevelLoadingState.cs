using System.Linq;
using Game.Level.Data;

namespace Game.Flow.Level.States
{
    public class LevelLoadingState : LevelStateBase 
    {
        public LevelLoadingState(LevelStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            Context.Reset();

            LevelConfig config = Context.Config;
            if (config == null) return;
            
            int totalKnivesToBreak = config.Targets.Sum(t => t.KnivesToBreak);
            Context.KnifeCount = config.KnifeCount > 0 ? config.KnifeCount : totalKnivesToBreak + config.ExtraKnives;

            // Phase 1: Spawn all elements (no positioning)
            var spawnedTargets = Context.LevelSpawner.SpawnTargets(config);
            
            // Phase 2: Position all attached elements
            Context.LevelSpawner.AttachElements(spawnedTargets, config, Context.StageConfig);
            
            // Phase 3: Finalize (animations, etc.)
            Context.LevelSpawner.FinalizeSpawning(spawnedTargets);

            Context.ActiveTargets.AddRange(spawnedTargets);

            Machine.SetState(Machine.Playing);
        }
    }
}
