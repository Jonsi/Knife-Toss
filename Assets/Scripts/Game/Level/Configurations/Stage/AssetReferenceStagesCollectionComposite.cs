using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Inspector-safe Addressables reference to a <see cref="StagesCollectionComposite"/> used for remote catalog loads.
    /// </summary>
    [Serializable]
    public class AssetReferenceStagesCollectionComposite : AssetReferenceT<StagesCollectionComposite>
    {
        /// <summary>
        /// Creates an empty reference for Unity serialization.
        /// </summary>
        public AssetReferenceStagesCollectionComposite()
            : base(string.Empty)
        {
        }

        /// <summary>
        /// Creates a reference from an asset GUID string.
        /// </summary>
        /// <param name="guid">Main asset GUID.</param>
        public AssetReferenceStagesCollectionComposite(string guid)
            : base(guid)
        {
        }
    }
}
