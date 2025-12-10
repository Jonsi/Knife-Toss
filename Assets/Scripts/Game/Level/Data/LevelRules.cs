using System;

namespace Game.Level.Data
{
    /// <summary>
    /// Immutable rules that govern a level session (knife allowance and win condition).
    /// </summary>
    public class LevelRules
    {
        public int KnifeCount { get; }
        public int StickThreshold { get; }

        public LevelRules(int knifeCount, int stickThreshold)
        {
            if (knifeCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(knifeCount), "Knife count must be greater than zero.");

            if (stickThreshold <= 0)
                throw new ArgumentOutOfRangeException(nameof(stickThreshold), "Stick threshold must be greater than zero.");

            KnifeCount = knifeCount;
            StickThreshold = stickThreshold;
        }
    }
}

