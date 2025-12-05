using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Represents the LevelCommandEventChannel class.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCommandEventChannel", menuName = "Knife Toss/Configurations/Events/Level Command Channel")]
    public class LevelCommandEventChannel : ScriptableObject
    {
        public event Action RetryRequested;
        public event Action NextRequested;
        public event Action<int> LevelRequested;

        /// <summary>
        /// Performs the RaiseRetry operation.
        /// </summary>
        public void RaiseRetry()
        {
            RetryRequested?.Invoke();
        }

        /// <summary>
        /// Performs the RaiseNext operation.
        /// </summary>
        public void RaiseNext()
        {
            NextRequested?.Invoke();
        }

        /// <summary>
        /// Raises a request to load a specific level.
        /// </summary>
        public void RaiseLevelRequest(int levelIndex)
        {
            LevelRequested?.Invoke(levelIndex);
        }
    }
}
