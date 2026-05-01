using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Ordered list of stage collections (worlds). Ordering defines progression and menus.
    /// </summary>
    [CreateAssetMenu(fileName = "StagesCollectionComposite", menuName = "Knife Toss/Gameplay/Stages Collection Composite")]
    public class StagesCollectionComposite : ScriptableObject
    {
        [Tooltip("Worlds in play and display order.")]
        [field: SerializeField]
        public List<StagesCollection> Collections { get; private set; } = new();

        /// <summary>
        /// Number of stage collections.
        /// </summary>
        public int CollectionsCount => Collections != null ? Collections.Count : 0;

        /// <summary>
        /// Returns the collection at the index, or null if out of range.
        /// </summary>
        /// <param name="collectionIndex">Zero-based collection index.</param>
        public StagesCollection GetCollection(int collectionIndex)
        {
            if (Collections == null || collectionIndex < 0 || collectionIndex >= Collections.Count)
            {
                return null;
            }

            return Collections[collectionIndex];
        }

        /// <summary>
        /// One-based stage number for UI, counting all stages in prior collections plus the local index.
        /// </summary>
        public int GetCumulativeStageNumberDisplay(int collectionIndex, int localStageIndex)
        {
            int sum = 0;
            if (Collections != null)
            {
                int limit = Math.Min(collectionIndex, Collections.Count);
                for (int i = 0; i < limit; i++)
                {
                    StagesCollection col = Collections[i];
                    sum += col?.Stages?.Count ?? 0;
                }
            }

            return sum + localStageIndex + 1;
        }
    }
}
