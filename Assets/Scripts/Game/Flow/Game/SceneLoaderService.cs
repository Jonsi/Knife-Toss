using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Game.Flow.Game
{
    /// <summary>
    /// Handles scene transitions and loading.
    /// </summary>
    public class SceneLoaderService
    {
        private const string MENU_SCENE = "StagesMenu";
        private const string GAME_SCENE = "StagePlay";

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        public async UniTask LoadMenu()
        {
            await SceneManager.LoadSceneAsync(MENU_SCENE, LoadSceneMode.Single).ToUniTask();
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
