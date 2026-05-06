using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Level.Configurations.Stage;
using Game.Level.Services;
using UnityEngine;
using VContainer;

namespace Game.UI.MainMenu
{
    /// <summary>
    /// Applies the active world's menu particle prefab under this transform when the main menu loads.
    /// </summary>
    public class MainMenuBackgroundEffect : MonoBehaviour
    {
        /// <summary>
        /// Resolves the player's current stage collection for themed background effects.
        /// </summary>
        [Inject]
        private StagesCollectionResolver _StagesCollectionResolver;

        /// <summary>
        /// Spawns and clears menu background particle instances.
        /// </summary>
        [Inject]
        private BackgroundEffectService _BackgroundEffectService;

        [Inject]
        private WorldAssetService _WorldAssetService;

        [Inject]
        private GameData _GameData;

        private void Start()
        {
            ApplyWhenReadyAsync().Forget();
        }

        private async UniTaskVoid ApplyWhenReadyAsync()
        {
            if (_WorldAssetService != null && _GameData != null)
            {
                await _WorldAssetService.LoadCollectionAsync(_GameData.CurrentCollectionIndex);
            }

            _BackgroundEffectService?.ApplyEffect(_StagesCollectionResolver?.GetActiveCollection(), transform);
        }
    }
}
