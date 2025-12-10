using System.Collections.Generic;
using KnifeToss.Common.Events;
using KnifeToss.Game.Knife.Data;
using KnifeToss.Game.Target.Interfaces;

namespace KnifeToss.Game.Level.States
{
    /// <summary>
    /// Level is actively being played.
    /// Manages throws, collisions, and win/loss conditions.
    /// Subscribes permanently to events but only acts when active to avoid collection modification errors.
    /// </summary>
    public class LevelPlayingState : LevelStateBase 
    {
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly IEventChannel<KnifeSpawnRequestData> _SpawnChannel;
        private readonly List<ITargetView> _SubscribedTargets = new List<ITargetView>();

        public LevelPlayingState(
            StageStateMachine machine,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<KnifeSpawnRequestData> spawnChannel)
            : base(machine)
        {
            _ImpactChannel = impactChannel;
            _SpawnChannel = spawnChannel;

            _ImpactChannel.Subscribe(OnKnifeImpact);
            _SpawnChannel.Subscribe(OnKnifeSpawned);
        }

        public override void Enter()
        {
            Context.IsThrowingActive = true;

            _SubscribedTargets.Clear();
            _SubscribedTargets.AddRange(Context.ActiveTargets);
            
            foreach (var target in _SubscribedTargets)
            {
                target.OnDestroyed += HandleTargetDestroyed;
            }
        }

        public override void Exit()
        {
            Context.IsThrowingActive = false;

            foreach (var target in _SubscribedTargets)
            {
                target.OnDestroyed -= HandleTargetDestroyed;
            }
            _SubscribedTargets.Clear();
        }

        private void OnKnifeSpawned(KnifeSpawnRequestData data)
        {
            if (machine.Current != this)
                return;

            Context.KnivesThrown++;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (machine.Current != this)
                return;

            if (data.impactType == KnifeImpactType.Bounced)
            {
                machine.SetState(machine.failed);
                return;
            }

            if (data.impactType == KnifeImpactType.Stuck)
            {
                Context.KnivesStuck++;
                CheckCompletion();
            }
        }

        private void HandleTargetDestroyed(ITargetView target)
        {
            Context.ActiveTargets.Remove(target);
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (Context.AreAllTargetsDestroyed)
            {
                machine.SetState(machine.completed);
                return;
            }

            if (!Context.HasKnivesRemaining)
            {
                machine.SetState(machine.failed);
            }
        }
    }
}
