using System;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    [CreateAssetMenu(fileName = "LevelCommandEventChannel", menuName = "Knife Toss/Events/Level Command Channel")]
    public class LevelCommandEventChannel : ScriptableObject
    {
        public event Action RetryRequested;
        public event Action NextRequested;

        public void RaiseRetry()
        {
            RetryRequested?.Invoke();
        }

        public void RaiseNext()
        {
            NextRequested?.Invoke();
        }
    }
}
