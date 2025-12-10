using System;
using KnifeToss.Core.Abstractions.State;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Domain;

namespace KnifeToss.Core.Domain.Level
{
    /// <summary>
    /// Tracks per-level session progress and transitions between level states.
    /// </summary>
    public sealed class LevelSession
    {
        private LevelRules _rules;
        private readonly GameStateMachine _stateMachine;

        public int KnivesThrown { get; private set; }
        public int KnivesStuck { get; private set; }

        public IState CurrentState => _stateMachine.Current;
        public bool IsCompleted => _stateMachine.Current is LevelCompletedState;
        public bool IsFailed => _stateMachine.Current is LevelFailedState;
        public string CurrentStateName => _stateMachine.CurrentStateName;
        public int KnifeCount => _rules.KnifeCount;
        public int StickThreshold => _rules.StickThreshold;
        public bool HasKnivesRemaining => KnivesThrown < _rules.KnifeCount;
        public bool IsPlaying => _stateMachine.IsPlaying;

        public LevelSession(LevelRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            var loading = new LevelLoadingState();
            var playing = new LevelPlayingState();
            var completed = new LevelCompletedState();
            var failed = new LevelFailedState();
            _stateMachine = new GameStateMachine(loading, playing, completed, failed);
        }

        public void Reset(LevelRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            KnivesThrown = 0;
            KnivesStuck = 0;
            _stateMachine.SetState(_stateMachine.Loading);
        }

        public void Start()
        {
            KnivesThrown = 0;
            KnivesStuck = 0;
            _stateMachine.SetState(_stateMachine.Playing);
        }

        public bool CanThrow() => IsPlaying && HasKnivesRemaining;

        public void RegisterThrow()
        {
            KnivesThrown++;
        }

        public void RegisterStick()
        {
            KnivesStuck++;
            // We now rely on external completion (via ForceComplete) OR threshold.
            // Keeping threshold check for safety/single-target backward compatibility.
            if (KnivesStuck >= _rules.StickThreshold)
            {
                _stateMachine.SetState(_stateMachine.Completed);
                return;
            }

            if (!HasKnivesRemaining)
            {
                _stateMachine.SetState(_stateMachine.Failed);
            }
        }

        public void ForceComplete()
        {
             if (IsPlaying)
             {
                 _stateMachine.SetState(_stateMachine.Completed);
             }
        }

        public void RegisterBounceFail()
        {
            _stateMachine.SetState(_stateMachine.Failed);
        }
    }
}
