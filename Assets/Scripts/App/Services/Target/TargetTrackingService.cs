using System;
using System.Collections.Generic;
using KnifeToss.Core.Abstractions.Gameplay.Target;

namespace App.Services.Target
{
    /// <summary>
    /// Tracks the state of active targets and signals level completion.
    /// </summary>
    public class TargetTrackingService
    {
        private readonly List<ITargetView> _activeTargets = new();
        
        /// <summary>
        /// Event raised when all registered targets have been destroyed.
        /// </summary>
        public event Action OnAllTargetsDestroyed;

        /// <summary>
        /// Registers a set of targets to track.
        /// </summary>
        /// <param name="targets">The collection of targets to monitor.</param>
        public void RegisterTargets(IEnumerable<ITargetView> targets)
        {
            _activeTargets.Clear();
            _activeTargets.AddRange(targets);

            foreach (var target in _activeTargets)
            {
                target.OnDestroyed += HandleTargetDestroyed;
            }
        }

        private void HandleTargetDestroyed(ITargetView destroyedTarget)
        {
            destroyedTarget.OnDestroyed -= HandleTargetDestroyed;
            _activeTargets.Remove(destroyedTarget);

            if (_activeTargets.Count == 0)
            {
                OnAllTargetsDestroyed?.Invoke();
            }
        }

        /// <summary>
        /// Checks if all registered targets are destroyed.
        /// </summary>
        /// <returns>True if no targets remain active.</returns>
        public bool AreAllTargetsDestroyed() => _activeTargets.Count == 0;
    }
}
