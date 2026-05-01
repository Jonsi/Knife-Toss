using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Identifies a stage reward slot within a specific world collection.
    /// </summary>
    [Serializable]
    public class StageRewardKey
    {
        /// <summary>
        /// Index of the stage collection (world).
        /// </summary>
        public int CollectionIndex;

        /// <summary>
        /// Stage index within that collection.
        /// </summary>
        public int StageIndex;
    }
}
