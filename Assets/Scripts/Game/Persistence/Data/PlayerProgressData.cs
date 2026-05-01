using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Represents player progression state stored between sessions.
    /// </summary>
    [Serializable]
    public class PlayerProgressData
    {
        public int CurrentCollectionIndex;
        public int CurrentStageIndex;
        public int HighestCollectionIndex;
        public int HighestStageIndex;
    }
}
