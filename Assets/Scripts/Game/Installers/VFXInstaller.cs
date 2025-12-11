using Common.Events;
using Game.Entities.Knife.Data;
using Game.Feedback.VFX.Data;
using Game.Feedback.VFX.Services;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs visual feedback systems.
    /// </summary>
    public class VFXInstaller : IInstaller
    {
        private readonly VFXSettings _VfxSettings;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VFXInstaller"/> class.
        /// </summary>
        public VFXInstaller(
            VFXSettings vfxSettings,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel)
        {
            _VfxSettings = vfxSettings;
            _KnifeImpactChannel = knifeImpactChannel;
        }

        /// <summary>
        /// Registers visual feedback services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_VfxSettings);
            builder.RegisterEntryPoint<VFXFeedbackService>();
        }
    }
}

