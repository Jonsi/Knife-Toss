using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Data;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Services;
using Game.Feedback.Audio.Systems;
using Game.Flow.Level;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs audio feedback systems.
    /// </summary>
    public class AudioInstaller : IInstaller
    {
        private readonly SoundSettings _SoundSettings;
        private readonly AudioPlayer _AudioPlayer;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly IEventChannel<LevelData> _LevelCompletedChannel;
        private readonly IEventChannel<LevelData> _LevelFailedChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioInstaller"/> class.
        /// </summary>
        public AudioInstaller(
            SoundSettings soundSettings,
            AudioPlayer audioPlayer,
            IEventChannel knifeThrownChannel,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IEventChannel<LevelData> levelCompletedChannel,
            IEventChannel<LevelData> levelFailedChannel)
        {
            _SoundSettings = soundSettings;
            _AudioPlayer = audioPlayer;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeImpactChannel = knifeImpactChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
        }

        /// <summary>
        /// Registers audio feedback services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_SoundSettings);
            builder.RegisterComponent(_AudioPlayer).As<IAudioPlayer>();

            builder.RegisterEntryPoint<KnifeAudioFeedbackService>();
            builder.RegisterEntryPoint<CollectibleAudioFeedbackService>();
            builder.RegisterEntryPoint<LevelAudioFeedbackService>();
        }
    }
}

