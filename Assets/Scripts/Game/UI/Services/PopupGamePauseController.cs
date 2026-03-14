using System.Collections.Generic;
using Game.UI.Interfaces;
using UnityEngine;

namespace Game.UI.Services
{
    /// <summary>
    /// Default popup pause policy. Keeps the game paused while any popup holds a pause request.
    /// </summary>
    public class PopupGamePauseController : IGamePauseController
    {
        private readonly HashSet<object> _PauseRequesters = new HashSet<object>();

        public void AcquirePause(object requester)
        {
            if (requester == null || !_PauseRequesters.Add(requester))
                return;

            if (_PauseRequesters.Count == 1)
                ApplyPausedState();
        }

        public void ReleasePause(object requester)
        {
            if (requester == null || !_PauseRequesters.Remove(requester))
                return;

            if (_PauseRequesters.Count == 0)
                ApplyResumedState();
        }

        protected virtual void ApplyPausedState()
        {
            Time.timeScale = 0f;
        }

        protected virtual void ApplyResumedState()
        {
            Time.timeScale = 1f;
        }
    }
}
