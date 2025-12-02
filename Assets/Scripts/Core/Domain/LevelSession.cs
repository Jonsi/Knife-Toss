using System;

namespace KnifeToss.Core.Domain
{



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

        private int preStuckBaseline;

        public void Begin(int levelIndex, LevelRules rules)
        {
            LevelIndex = levelIndex;
            Rules = rules;
            KnivesStuck = 0;
            KnivesThrown = 0;
            IsComplete = false;
            IsFailed = false;
            preStuckBaseline = 0;
            NotifyChanged();
        }

        public void SeedPreStuckKnives(int count)
        {
            if (count <= 0)
                return;

            KnivesStuck = count;
            preStuckBaseline = KnivesStuck;
            EvaluateProgress();
            NotifyChanged();
        }

        public void RecordThrow()
        {
            if (IsComplete || IsFailed)
                return;

            KnivesThrown++;
            NotifyChanged();
        }

        public void RecordKnifeStuck()
        {
            if (IsComplete || IsFailed)
                return;

            KnivesStuck++;
            EvaluateProgress();
            NotifyChanged();
        }

        public void RecordFailure()
        {
            if (IsComplete || IsFailed)
                return;

            IsFailed = true;
            NotifyChanged();
            Failed?.Invoke(this);
        }

        public bool HasExhaustedKnives()
        {
            if (Rules.MaxKnives <= 0)
                return false;

            return KnivesThrown >= Rules.MaxKnives && KnivesStuck < Rules.RequiredKnives;
        }

        private void EvaluateProgress()
        {
            if (PlayerKnivesStuck >= Rules.RequiredKnives)
            {
                IsComplete = true;
                Completed?.Invoke(this);
            }
        }

        private void NotifyChanged()
        {
            SessionChanged?.Invoke(this);
        }
    }
}
