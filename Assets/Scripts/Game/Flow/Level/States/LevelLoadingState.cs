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

            var spawnedTargets = Context.LevelSpawner.SpawnLevel(config, Context.StageConfig);
            Context.ActiveTargets.AddRange(spawnedTargets);

            Machine.SetState(Machine.Playing);
        }
    }
}
