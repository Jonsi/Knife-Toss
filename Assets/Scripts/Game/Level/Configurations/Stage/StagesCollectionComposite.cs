using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Ordered list of stage collections (worlds). Ordering defines progression and menus.
    /// </summary>
    [CreateAssetMenu(fileName = "StagesCollectionComposite", menuName = "Knife Toss/Gameplay/Stages Collection Composite")]
    public class StagesCollectionComposite : ScriptableObject
    {
        private const int StageNumberDisplayOffset = 1;
        private const int FirstWorldIndex = 0;
        private const int EmptyStageSlotCount = 0;

        [Header("Worlds (Addressables)")]
        [SerializeField] private List<AssetReferenceT<StagesCollection>> _CollectionReferences = new();

        [Tooltip("Stage slot counts per world, same order as collection references. Used for progression and UI when collections are not addressable-loaded.")]
        [SerializeField] private List<int> _StageSlotCountsPerWorld = new();

        /// <summary>
        /// Number of stage collections.
        /// </summary>
        public int CollectionsCount => _CollectionReferences != null ? _CollectionReferences.Count : 0;

        /// <summary>
        /// Returns the Addressables location / runtime key for the world at the index, or null when missing.
        /// </summary>
        /// <param name="collectionIndex">Zero-based collection index.</param>
        public string GetCollectionLocationKey(int collectionIndex)
        {
            if (_CollectionReferences == null ||
                collectionIndex < FirstWorldIndex ||
                collectionIndex >= _CollectionReferences.Count)
            {
                return null;
            }

            AssetReferenceT<StagesCollection> reference = _CollectionReferences[collectionIndex];
            if (reference == null || !reference.RuntimeKeyIsValid())
            {
                return null;
            }

            return reference.RuntimeKey.ToString();
        }

        /// <summary>
        /// Configured stage slot count for a world (for unlock math without loading the collection asset).
        /// </summary>
        public int GetStageSlotCountForWorld(int collectionIndex)
        {
            if (_StageSlotCountsPerWorld == null ||
                collectionIndex < FirstWorldIndex ||
                collectionIndex >= _StageSlotCountsPerWorld.Count)
            {
                return EmptyStageSlotCount;
            }

            return Mathf.Max(EmptyStageSlotCount, _StageSlotCountsPerWorld[collectionIndex]);
        }

        /// <summary>
        /// One-based stage number for UI, counting all stages in prior collections plus the local index.
        /// </summary>
        public int GetCumulativeStageNumberDisplay(int collectionIndex, int localStageIndex)
        {
            int sum = 0;
            if (_CollectionReferences != null)
            {
                int limit = Math.Min(collectionIndex, CollectionsCount);
                for (int i = FirstWorldIndex; i < limit; i++)
                {
                    sum += GetStageSlotCountForWorld(i);
                }
            }

            return sum + localStageIndex + StageNumberDisplayOffset;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: resolves the project asset for a slot without Addressables.
        /// </summary>
        public StagesCollection GetEditorOnlyCollection(int collectionIndex)
        {
            if (_CollectionReferences == null ||
                collectionIndex < FirstWorldIndex ||
                collectionIndex >= _CollectionReferences.Count)
            {
                return null;
            }

            AssetReferenceT<StagesCollection> reference = _CollectionReferences[collectionIndex];
            return reference != null ? reference.editorAsset as StagesCollection : null;
        }
#endif

        private void OnValidate()
        {
            if (_CollectionReferences == null)
            {
                return;
            }

            if (_StageSlotCountsPerWorld == null)
            {
                _StageSlotCountsPerWorld = new List<int>();
            }

            while (_StageSlotCountsPerWorld.Count < _CollectionReferences.Count)
            {
                _StageSlotCountsPerWorld.Add(EmptyStageSlotCount);
            }

            while (_StageSlotCountsPerWorld.Count > _CollectionReferences.Count)
            {
                _StageSlotCountsPerWorld.RemoveAt(_StageSlotCountsPerWorld.Count - 1);
            }
        }
    }
}
