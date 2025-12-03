using System;
using System.Collections.Generic;

namespace KnifeToss.Core.Data
{
    /// <summary>
    /// Represents the LevelProgressData class.
    /// </summary>
    [Serializable]
    public class LevelProgressData
    {
        public Dictionary<int, int> starsPerLevel = new Dictionary<int, int>();
    }
}
