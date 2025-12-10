using System.Collections.Generic;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Target.Interfaces;
using Game.KnifeThrower.Interfaces;

namespace Game.Flow.Level.States
{
    public class LevelPlayingState : LevelStateBase 
    {
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly IKnifeThrowerView _KnifeThrowerView;
        private readonly List<ITargetView> _SubscribedTargets = new List<ITargetView>();

        public LevelPlayingState(
            LevelStateMachine machine,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IKnifeThrowerView knifeThrowerView)
            : base(machine)
        {
            _ImpactChannel = impactChannel;
            _KnifeThrowerView = knifeThrowerView;

            _ImpactChannel.Subscribe(OnKnifeImpact);
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

            if (_KnifeThrowerView != null)
                _KnifeThrowerView.OnKnifeSpawned += OnKnifeSpawned;
        }

        public override void Exit()
        {
            Context.IsThrowingActive = false;

            foreach (var target in _SubscribedTargets)
            {
                target.OnDestroyed -= HandleTargetDestroyed;
            }
            _SubscribedTargets.Clear();

            if (_KnifeThrowerView != null)
                _KnifeThrowerView.OnKnifeSpawned -= OnKnifeSpawned;
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
