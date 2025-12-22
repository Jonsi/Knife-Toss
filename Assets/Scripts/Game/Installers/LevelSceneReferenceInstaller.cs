using Game.KnifeThrower.Interfaces;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Systems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Registers scene-bound components explicitly to avoid hierarchy scans.
    /// </summary>
    public class LevelSceneReferenceInstaller : MonoBehaviour, IInstaller
    {
        [Header("Scene References")]
        [SerializeField] private SceneReferences _SceneReferences;
        [SerializeField] private AudioPlayer _AudioPlayer;

        public void Install(IContainerBuilder builder)
        {
            RegisterScene(builder, _SceneReferences);

            if (_AudioPlayer != null)
            {
                builder.RegisterComponent(_AudioPlayer).As<IAudioPlayer>();
            }
        }

        private static void RegisterScene(IContainerBuilder builder, SceneReferences refs)
        {
            if (refs == null)
                return;

            builder.RegisterComponent(refs);
            RegisterComponent(builder, refs.UIController);
            RegisterComponent(builder, refs.KnifeMagazineView);
            RegisterComponent(builder, refs.StageProgressView);
            RegisterComponent(builder, refs.CollectibleCountView);
            RegisterComponent(builder, refs.MuteButtonView);
            RegisterComponent(builder, refs.PowerUpHudView);
            if (refs.KnifeThrowerView != null)
            {
                builder.RegisterComponent(refs.KnifeThrowerView).As<IKnifeThrowerView>();
            }
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component) where T : Component
        {
            if (component != null)
            {
                builder.RegisterComponent(component);
            }
        }
    }
}
