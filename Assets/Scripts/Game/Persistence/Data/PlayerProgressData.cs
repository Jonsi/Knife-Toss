using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Represents player progression state stored between sessions.
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        public int CurrentStageIndex;
        public int HighestStageIndex;
    }
}
