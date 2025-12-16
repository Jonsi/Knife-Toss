using System.Collections.Generic;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Target.Interfaces;
using Game.Flow.Game;
using Game.KnifeThrower.Interfaces;
using Game.Entities.Collectible.Data;

namespace Game.Flow.Level.States
{
    public class LevelPlayingState : LevelStateBase 
    {
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly IKnifeThrowerView _KnifeThrowerView;
        private readonly GameData _GameData;
        private readonly List<ITargetView> _SubscribedTargets = new List<ITargetView>();

        public LevelPlayingState(
            LevelStateMachine machine,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IKnifeThrowerView knifeThrowerView,
            GameData gameData)
            : base(machine)
        {
            _ImpactChannel = impactChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _KnifeThrowerView = knifeThrowerView;
            _GameData = gameData;
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
        
        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            if (Machine.Current != this)
                return;

            Context.CollectedCount += data.Value;
            
            if (_GameData != null)
            {
                _GameData.TotalCollected += data.Value;
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
