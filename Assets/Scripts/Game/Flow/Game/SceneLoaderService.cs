using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Flow.Game
{
    /// <summary>
    /// Handles scene transitions and loading.
    /// </summary>
    public class SceneLoaderService
    {
        private const string MAIN_MENU_SCENE = "MainMenu";
        private const string STAGES_MENU_SCENE = "StagesMenu";
        private const string STORE_SCENE = "StoreScene";
        private const string POWER_UPS_SCENE = "PowerUpsMenu";
        private const string GAME_SCENE = "StagePlay";
        private const string PVP_SCENE = "PVPPlay";

        private UniTask _PreloadGameTask;
        private bool _IsPreloadingGame;
        private bool _IsGamePreloaded;
        private int _PreloadGeneration;

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public async UniTask LoadMainMenu()
        {
            InvalidatePreloadedGameState();
            await SceneManager.LoadSceneAsync(MAIN_MENU_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        /// <summary>
        /// Loads the stages selection menu scene.
        /// </summary>
        public async UniTask LoadStagesMenu()
        {
            InvalidatePreloadedGameState();
            await SceneManager.LoadSceneAsync(STAGES_MENU_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        /// <summary>
        /// Loads the store scene.
        /// </summary>
        public async UniTask LoadStore()
        {
            InvalidatePreloadedGameState();
            await SceneManager.LoadSceneAsync(STORE_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        /// <summary>
        /// Loads the power-ups menu scene.
        /// </summary>
        public async UniTask LoadPowerUps()
        {
            InvalidatePreloadedGameState();
            await SceneManager.LoadSceneAsync(POWER_UPS_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        /// <summary>
        /// Loads the game (stage play) scene.
        /// </summary>
        public async UniTask LoadGame()
        {
            InvalidatePreloadedGameState();
            await SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        public UniTask PreloadGameFromMainMenu()
        {
            if (_IsGamePreloaded)
            {
                return UniTask.CompletedTask;
            }

            if (_IsPreloadingGame)
            {
                return _PreloadGameTask;
            }

            _PreloadGameTask = PreloadGameFromMainMenuInternal();
            return _PreloadGameTask;
        }

        public async UniTask RevealPreloadedGameFromMainMenu()
        {
            if (!HasLoadedGameScene())
            {
                _IsGamePreloaded = false;
            }

            if (!_IsGamePreloaded)
            {
                await PreloadGameFromMainMenu();
            }

            Scene stageScene = SceneManager.GetSceneByName(GAME_SCENE);
            if (!stageScene.IsValid() || !stageScene.isLoaded)
            {
                await LoadGame();
                return;
            }

            if (stageScene.IsValid())
            {
                SceneManager.SetActiveScene(stageScene);
            }

            DeferredGameSceneVisibility.RevealLoadedScene();

            Scene mainMenuScene = SceneManager.GetSceneByName(MAIN_MENU_SCENE);
            if (mainMenuScene.IsValid() && mainMenuScene.isLoaded && SceneManager.sceneCount > 1)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(MAIN_MENU_SCENE);
                if (unloadOperation != null)
                {
                    await unloadOperation.ToUniTask();
                }
            }

            _IsGamePreloaded = false;
            _PreloadGameTask = default;
        }

        private async UniTask PreloadGameFromMainMenuInternal()
        {
            int preloadGeneration = _PreloadGeneration;
            _IsPreloadingGame = true;
            DeferredGameSceneVisibility.BeginHiddenLoad();

            try
            {
                Scene existingScene = SceneManager.GetSceneByName(GAME_SCENE);
                if (existingScene.IsValid() && existingScene.isLoaded)
                {
                    if (preloadGeneration == _PreloadGeneration)
                    {
                        _IsGamePreloaded = true;
                    }

                    return;
                }

                await SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Additive).ToUniTask();
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                if (preloadGeneration == _PreloadGeneration && HasLoadedGameScene())
                {
                    _IsGamePreloaded = true;
                }
            }
            finally
            {
                if (preloadGeneration == _PreloadGeneration)
                {
                    _IsPreloadingGame = false;
                }
            }
        }

        /// <summary>
        /// Loads the PVP game scene.
        /// </summary>
        public async UniTask LoadPVP()
        {
            InvalidatePreloadedGameState();
            await SceneManager.LoadSceneAsync(PVP_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        private bool HasLoadedGameScene()
        {
            Scene gameScene = SceneManager.GetSceneByName(GAME_SCENE);
            return gameScene.IsValid() && gameScene.isLoaded;
        }

        private void InvalidatePreloadedGameState()
        {
            _PreloadGeneration++;
            _IsPreloadingGame = false;
            _IsGamePreloaded = false;
            _PreloadGameTask = default;
            DeferredGameSceneVisibility.Reset();
        }
    }
}
