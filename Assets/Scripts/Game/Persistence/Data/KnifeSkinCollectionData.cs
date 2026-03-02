using System;
using System.Collections.Generic;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Stores permanently unlocked knife skins and the currently equipped skin.
    /// </summary>
    [Serializable]
    public class KnifeSkinCollectionData
    {
        public List<string> UnlockedIds = new List<string>();
        public string EquippedId;

        public bool IsUnlocked(string skinId)
        {
            return !string.IsNullOrEmpty(skinId) && UnlockedIds.Contains(skinId);
        }

        public bool Unlock(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) || UnlockedIds.Contains(skinId))
            {
                return false;
            }

            UnlockedIds.Add(skinId);
            return true;
        }

        public bool Equip(string skinId)
        {
            if (!IsUnlocked(skinId))
            {
                return false;
            }

            EquippedId = skinId;
            return true;
        }
    }
}
