using System;
using Core.Abstractions;
using UnityEngine;
using VContainer.Unity;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// Bridges Unity lifecycle events to the player data service for safe saves.
    /// </summary>
    public sealed class PlayerDataLifecycle : IStartable, IDisposable
    {
        private readonly IPlayerDataService playerDataService;
        private SaveHook hook;
        private bool ownsHook;

        public PlayerDataLifecycle(IPlayerDataService playerDataService)
        {
            this.playerDataService = playerDataService;
        }

        public void Start()
        {
            Application.quitting += HandleApplicationQuit;
            hook = UnityEngine.Object.FindFirstObjectByType<SaveHook>();
            if (hook == null)
            {
                hook = new GameObject("PlayerDataSaveHook").AddComponent<SaveHook>();
                hook.gameObject.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(hook);
                ownsHook = true;
            }
            hook.Initialize(playerDataService);
        }

        public void Dispose()
        {
            Application.quitting -= HandleApplicationQuit;

            if (ownsHook && hook != null)
            {
                UnityEngine.Object.Destroy(hook.gameObject);
                hook = null;
            }
        }

        private void HandleApplicationQuit()
        {
            playerDataService.Save();
        }

        private sealed class SaveHook : MonoBehaviour
        {
            private IPlayerDataService service;

            public void Initialize(IPlayerDataService playerDataService)
            {
                service = playerDataService;
            }

            private void OnApplicationPause(bool pauseStatus)
            {
                if (pauseStatus)
                {
                    service?.Save();
                }
            }
        }
    }
}
