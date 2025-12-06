using System;
using Core.Data;

namespace Core.Domain
{



    /// <summary>
    /// Represents the LevelSession class.
    /// </summary>
    public class LevelSession
    {
        public event Action<LevelSession> SessionChanged;
        public event Action<LevelSession> Completed;
        public event Action<LevelSession> Failed;

        public int LevelIndex { get; private set; }
        public LevelRules Rules { get; private set; }
        public int KnivesStuck { get; private set; }
        public int KnivesThrown { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsFailed { get; private set; }
        public int PlayerKnivesStuck => Math.Max(0, KnivesStuck - preStuckBaseline);
        public int Score { get; private set; }
        public bool HadToRetry { get; private set; }
        public int TargetCount { get; private set; }
        public int TargetsRemaining { get; private set; }
        public int HitableObjectsHit { get; private set; }

        private int preStuckBaseline;

        /// <summary>
        /// Performs the Begin operation.
        /// </summary>
        public void Begin(int levelIndex, LevelRules rules)
        {
            LevelIndex = levelIndex;
            Rules = rules;
            KnivesStuck = 0;
            KnivesThrown = 0;
            IsComplete = false;
            IsFailed = false;
            preStuckBaseline = 0;
            Score = 0;
            HadToRetry = false;
            TargetCount = Math.Max(1, rules.TargetCount);
            TargetsRemaining = TargetCount;
            HitableObjectsHit = 0;
            NotifyChanged();
        }

        /// <summary>
        /// Performs the SeedPreStuckKnives operation.
        /// </summary>
        public void SeedPreStuckKnives(int count)
        {
            if (count <= 0)
                return;

            KnivesStuck = count;
            preStuckBaseline = KnivesStuck;
            EvaluateProgress();
            NotifyChanged();
        }

        /// <summary>
        /// Performs the RecordThrow operation.
        /// </summary>
        public void RecordThrow()
        {
            if (IsComplete || IsFailed)
                return;

            KnivesThrown++;
            NotifyChanged();
        }

        /// <summary>
        /// Performs the RecordKnifeStuck operation.
        /// </summary>
        public void RecordKnifeStuck()
        {
            if (IsFailed)
                return;

            KnivesStuck++;
            EvaluateProgress();
            NotifyChanged();
        }

        /// <summary>
        /// Performs the RecordHitableObjectHit operation.
        /// </summary>
        public void RecordHitableObjectHit()
        {
            if (IsFailed)
            {
                return;
            }

            HitableObjectsHit++;
            EvaluateProgress();
            NotifyChanged();
        }

        /// <summary>
        /// Performs the AddScore operation.
        /// </summary>
        public void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Score += amount;
            NotifyChanged();
        }

        /// <summary>
        /// Performs the MarkTargetCompleted operation.
        /// </summary>
        public void MarkTargetCompleted()
        {
            TargetsRemaining = Math.Max(0, TargetsRemaining - 1);
            NotifyChanged();
        }

        /// <summary>
        /// Performs the RecordFailure operation.
        /// </summary>
        public void RecordFailure()
        {
            if (IsComplete || IsFailed)
                return;

            IsFailed = true;
            HadToRetry = true;
            NotifyChanged();
            Failed?.Invoke(this);
        }

        /// <summary>
        /// Performs the HasExhaustedKnives operation.
        /// </summary>
        public bool HasExhaustedKnives()
        {
            return KnivesThrown >= Rules.KnifeCount;
        }

        /// <summary>
        /// Performs the EvaluateProgress operation.
        /// </summary>
        private void EvaluateProgress()
        {
            // Progress evaluation removed - no stars/objectives
        }

        /// <summary>
        /// Performs the NotifyChanged operation.
        /// </summary>
        private void NotifyChanged()
        {
            SessionChanged?.Invoke(this);
        }

        /// <summary>
        /// Marks the session complete when all targets are destroyed.
        /// </summary>
        public void MarkCompleteIfNeeded()
        {
            if (IsComplete)
            {
                return;
            }

            // Complete when all targets are destroyed
            if (TargetsRemaining <= 0)
            {
                IsComplete = true;
                Completed?.Invoke(this);
            }
        }
    }
}
