using Common.Events;
using Common.Patterns.State;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Data;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.KnifeThrower.Interfaces;
using VContainer.Unity;

namespace Game.Flow.Level.States
{
    public class LevelStateMachine : StateMachine, ITickable
    {
        public LevelData Context { get; }

        public readonly LevelLoadingState Loading;
        public readonly LevelPlayingState Playing;
        public readonly LevelCompletedState Completed;
        public readonly LevelFailedState Failed;

        private readonly LevelCompletedEventChannel _LevelCompletedChannel;
        private readonly LevelFailedEventChannel _LevelFailedChannel;

        /// <summary>
        /// Initializes a new instance of the LevelStateMachine.
        /// </summary>
        public LevelStateMachine(
            LevelData context,
            LevelCompletedEventChannel levelCompletedChannel,
            LevelFailedEventChannel levelFailedChannel,
            IEventChannel<KnifeImpactEventData> impactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IEventChannel knifeThrownChannel,
            IKnifeThrowerView knifeThrowerView)
        {
            Context = context;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;

            Loading = new LevelLoadingState(this);
            Playing = new LevelPlayingState(this, impactChannel, collectibleHitChannel, knifeThrownChannel, knifeThrowerView);
            Completed = new LevelCompletedState(this);
            Failed = new LevelFailedState(this);
        }

        public void NotifyLevelCompleted()
        {
            _LevelCompletedChannel.Raise(Context);
        }

        public void NotifyLevelFailed()
        {
            _LevelFailedChannel.Raise(Context);
        }

        public void Tick()
        {
            Update(UnityEngine.Time.deltaTime);
        }
    }
}