using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Represents the LevelProgressData class.
    /// </summary>
    [Serializable]
    public class LevelProgressData : ISerializationCallbackReceiver
    {
        [SerializeField] private List<int> levelIndices = new List<int>();
        [SerializeField] private List<int> stars = new List<int>();

        [NonSerialized] public Dictionary<int, int> starsPerLevel = new Dictionary<int, int>();

        public int GetStars(int levelIndex)
        {
            return starsPerLevel.TryGetValue(levelIndex, out var starCount) ? starCount : 0;
        }

        public void SetStars(int levelIndex, int starCount)
        {
            starsPerLevel[levelIndex] = Mathf.Max(0, starCount);
        }

        public void Clear()
        {
            starsPerLevel.Clear();
        }

        public void OnBeforeSerialize()
        {
            levelIndices.Clear();
            stars.Clear();

            foreach (var kvp in starsPerLevel)
            {
                levelIndices.Add(kvp.Key);
                stars.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            starsPerLevel = new Dictionary<int, int>();
            int count = Mathf.Min(levelIndices.Count, stars.Count);
            for (int i = 0; i < count; i++)
            {
                starsPerLevel[levelIndices[i]] = stars[i];
            }
        }
    }
}
