using Game.DI;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Services;
using Game.Feedback.Audio.Systems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs audio feedback systems.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioInstaller", menuName = "Knife Toss/Installers/Audio")]
    public class AudioInstallerSO : BaseInstallerSO
    {
        [Header("Settings")]
        [SerializeField] private SoundSettings _SoundSettings;

        /// <summary>
        /// Registers audio feedback services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_SoundSettings);
            builder.Register<KnifeAudioFeedbackService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CollectibleAudioFeedbackService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LevelAudioFeedbackService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}

