using Cysharp.Threading.Tasks;
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
        private const string GAME_SCENE = "StagePlay";

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public async UniTask LoadMainMenu()
        {
            await SceneManager.LoadSceneAsync(MAIN_MENU_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        /// <summary>
        /// Loads the stages selection menu scene.
        /// </summary>
        public async UniTask LoadStagesMenu()
        {
            await SceneManager.LoadSceneAsync(STAGES_MENU_SCENE, LoadSceneMode.Single).ToUniTask();
        }

        /// <summary>
        /// Loads the game (stage play) scene.
        /// </summary>
        public async UniTask LoadGame()
        {
            await SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Single).ToUniTask();
        }
    }
}
