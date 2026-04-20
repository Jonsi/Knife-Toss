using System;
using System.Collections.Generic;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Persisted ownership list for non-consumable IAP product keys.
    /// </summary>
    [Serializable]
    public class IapEntitlementsData
    {
        public List<string> OwnedProductKeys = new List<string>();

        public bool IsOwned(string productKey)
        {
            return !string.IsNullOrEmpty(productKey) && OwnedProductKeys.Contains(productKey);
        }

        public bool SetOwned(string productKey, bool owned)
        {
            if (string.IsNullOrEmpty(productKey))
                return false;

            var alreadyOwned = OwnedProductKeys.Contains(productKey);
            if (owned)
            {
                if (alreadyOwned)
                    return false;

                OwnedProductKeys.Add(productKey);
                return true;
            }

            if (!alreadyOwned)
                return false;

            OwnedProductKeys.Remove(productKey);
            return true;
        }
    }
}
