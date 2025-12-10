using System.Collections.Generic;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay.Target;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Data.Throwing;
using KnifeToss.Core.Domain;
using UnityEngine;

namespace KnifeToss.Core.Domain.Level
{
    /// <summary>
    /// Level is actively being played.
    /// Manages throws, collisions, and win/loss conditions.
    /// Subscribes permanently to events but only acts when active to avoid collection modification errors.
    /// </summary>
    public class LevelPlayingState : LevelStateBase 
    {
        private readonly IEventChannel<KnifeImpactEventData> _impactChannel;
        private readonly IEventChannel<KnifeSpawnRequestData> _spawnChannel;
        private readonly List<ITargetView> _subscribedTargets = new List<ITargetView>();

        public LevelPlayingState(
            StageStateMachine machine, 
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<KnifeSpawnRequestData> spawnChannel) 
            : base(machine) 
        {
            _impactChannel = impactChannel;
            _spawnChannel = spawnChannel;

            _impactChannel.Subscribe(OnKnifeImpact);
            _spawnChannel.Subscribe(OnKnifeSpawned);
        }

        public override void Enter()
        {
            Context.IsThrowingActive = true;

            _subscribedTargets.Clear();
            _subscribedTargets.AddRange(Context.ActiveTargets);
            
            foreach (var target in _subscribedTargets)
            {
                target.OnDestroyed += HandleTargetDestroyed;
            }
        }

        public override void Exit()
        {
            Context.IsThrowingActive = false;

            foreach (var target in _subscribedTargets)
            {
                target.OnDestroyed -= HandleTargetDestroyed;
            }
            _subscribedTargets.Clear();
        }

        private void OnKnifeSpawned(KnifeSpawnRequestData data)
        {
            if (Machine.Current != this)
                return;

            Context.KnivesThrown++;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (Machine.Current != this)
                return;

            if (data.ImpactType == KnifeImpactType.Bounced)
            {
                Machine.SetState(Machine.Failed);
                return;
            }

            if (data.ImpactType == KnifeImpactType.Stuck)
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
                Machine.SetState(Machine.Completed);
                return;
            }

            if (!Context.HasKnivesRemaining)
            {
                Machine.SetState(Machine.Failed);
            }
        }
    }
}
