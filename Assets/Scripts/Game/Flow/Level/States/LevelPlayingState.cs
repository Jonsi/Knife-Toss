using System.Collections.Generic;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Data;
using Game.Entities.Target.Interfaces;
using Game.KnifeThrower.Interfaces;

namespace Game.Flow.Level.States
{
    public class LevelPlayingState : LevelStateBase
    {
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IKnifeThrowerView _KnifeThrowerView;
        private readonly List<ITargetView> _SubscribedTargets = new List<ITargetView>();

        /// <summary>
        /// Initializes a new instance of the LevelPlayingState.
        /// </summary>
        public LevelPlayingState(
            LevelStateMachine machine,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IEventChannel knifeThrownChannel,
            IKnifeThrowerView knifeThrowerView)
            : base(machine)
        {
            _ImpactChannel = impactChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeThrowerView = knifeThrowerView;
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


            _ImpactChannel.Subscribe(OnKnifeImpact);
            if (_CollectibleHitChannel != null)
                _CollectibleHitChannel.Subscribe(OnCollectibleHit);

            if (_KnifeThrownChannel != null)
                _KnifeThrownChannel.Subscribe(OnKnifeThrown);
        }

        public override void Exit()
        {
            Context.IsThrowingActive = false;

            foreach (var target in _SubscribedTargets)
            {
                target.OnDestroyed -= HandleTargetDestroyed;
            }
            _SubscribedTargets.Clear();


            _ImpactChannel.Unsubscribe(OnKnifeImpact);
            if (_CollectibleHitChannel != null)
                _CollectibleHitChannel.Unsubscribe(OnCollectibleHit);

            if (_KnifeThrownChannel != null)
                _KnifeThrownChannel.Unsubscribe(OnKnifeThrown);
        }

        private void OnKnifeThrown()
        {
            if (Machine.Current != this)
                return;

            Context.KnivesThrown++;
            Context.KnivesInFlight++;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (Machine.Current != this)
                return;

            switch (data.ImpactType)
            {
                case KnifeImpactType.Stuck:
                    Context.KnivesInFlight = UnityEngine.Mathf.Max(0, Context.KnivesInFlight - 1);
                    Context.KnivesStuck++;
                    break;
                case KnifeImpactType.OutOfView:
                    Context.KnivesInFlight = UnityEngine.Mathf.Max(0, Context.KnivesInFlight - 1);
                    break;
                case KnifeImpactType.Bounced:
                    Context.HasBounced |= data.CountsAsFailure;
                    break;
            }

            CheckCompletion();
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            if (Machine.Current != this)
                return;
            Context.CollectedCount += data.Value;
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

            // Only check for failure after all launched knives have finished their lifecycle
            if (Context.KnivesInFlight > 0)
                return;

            if (Context.HasBounced || !Context.HasKnivesRemaining)
            {
                Machine.SetState(Machine.Failed);
            }
        }
    }
}
