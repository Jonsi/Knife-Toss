using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Stores counts for each available power-up type using a dictionary-based approach.
    /// </summary>
    [Serializable]
    public class PowerUpInventoryData
    {
        [SerializeField] [JsonProperty("_Entries")] private List<PowerUpInventoryEntry> _Entries = new List<PowerUpInventoryEntry>();

        /// <summary>
        /// Gets the count for the specified power-up key.
        /// </summary>
        public int GetCount(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;

            var entry = _Entries.FirstOrDefault(e => e.Key == key);
            return entry?.Count ?? 0;
        }

        /// <summary>
        /// Sets the count for the specified power-up key.
        /// </summary>
        public void SetCount(string key, int count)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[PowerUpInventoryData] SetCount called with null or empty key.");
                return;
            }

            count = Mathf.Max(0, count);
            var entry = _Entries.FirstOrDefault(e => e.Key == key);
            if (entry != null)
            {
                entry.Count = count;
            }
            else
            {
                _Entries.Add(new PowerUpInventoryEntry { Key = key, Count = count });
            }
        }

        /// <summary>
        /// Increments the count for the specified power-up key.
        /// </summary>
        public void IncrementCount(string key, int amount = 1)
        {
            SetCount(key, GetCount(key) + amount);
        }

        /// <summary>
        /// Decrements the count for the specified power-up key.
        /// </summary>
        public void DecrementCount(string key, int amount = 1)
        {
            SetCount(key, GetCount(key) - amount);
        }
    }

    [Serializable]
    internal class PowerUpInventoryEntry
    {
        public string Key;
        public int Count;
    }
}