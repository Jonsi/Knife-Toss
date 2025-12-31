using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using VContainer.Unity;

namespace Game.Flow.Loading
{
    /// <summary>
    /// Controls the loading screen flow. Transitions to MainMenu after global services are initialized.
    /// </summary>
    public class LoadingController : IStartable
    {
        private readonly SceneLoaderService _SceneLoaderService;

        public LoadingController(SceneLoaderService sceneLoaderService)
        {
            _SceneLoaderService = sceneLoaderService;
        }

        public void Start()
        {
            TransitionToMainMenu().Forget();
        }

        private async UniTask TransitionToMainMenu()
        {
            await UniTask.Yield();
            await _SceneLoaderService.LoadMainMenu();
        }
    }
}
