using Common.Collision.Events;
using Common.Events;
using Game.Entities.Knife.Events;
using Game.Entities.Knife.Views;
using Game.Installers;
using Game.PVP.Configs;
using Game.PVP.Flow;
using Game.PVP.Input;
using Game.PVP.Services;
using Game.PVP.UI;
using Game.Pooling;
using Game.KnifeThrower.Events;
using Game.Entities.Collectible.Events;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Game.Entities.Knife.Data;
using Game.Flow.Events;
using Game.Input;

namespace Game.PVP.Installers
{
    public class PVPInstaller : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private PVPSettingsConfig _PvpConfig;
        [SerializeField] private PrefabsConfiguration _PrefabsConfiguration;

        [Header("Scene References")]
        [SerializeField] private KnifeThrowerView _Player1Thrower;
        [SerializeField] private KnifeThrowerView _Player2Thrower;
        [SerializeField] private PVPFlowController _FlowController;
        [SerializeField] private PVPHudController _HudController;

        [Header("Event Channels")]
        [SerializeField] private CollisionEventChannel _CollisionEventChannel;
        [SerializeField] private KnifeImpactEventChannel _KnifeImpactEventChannel;
        [SerializeField] private KnifeThrownEventChannel _KnifeThrownEventChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitEventChannel;
        
        // Optional if we wanted to use event-based navigation
        // [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;

        protected override void Configure(IContainerBuilder builder)
        {
            // Configs
            builder.RegisterInstance(_PvpConfig);
            builder.RegisterInstance(_PrefabsConfiguration);

            // Event Channels
            builder.RegisterInstance(_KnifeImpactEventChannel).As<IEventChannel<KnifeImpactEventData>>();
            builder.RegisterInstance(_KnifeThrownEventChannel).AsImplementedInterfaces();

            // Services
            builder.Register<PVPThrowerManager>(Lifetime.Scoped)
                .WithParameter("player1Thrower", _Player1Thrower)
                .WithParameter("player2Thrower", _Player2Thrower);
                
            builder.Register<InputAdapter>(Lifetime.Scoped).AsSelf();
            builder.Register<PVPInputController>(Lifetime.Scoped);

            // Installers
            Install(builder, new CollisionInstaller(_CollisionEventChannel));
            Install(builder, new CollectibleInstaller(_CollectibleHitEventChannel));
            Install(builder, new VFXInstaller(_PrefabsConfiguration, _KnifeImpactEventChannel));
            Install(builder, new TargetInstaller(_PrefabsConfiguration));
            
            // UI
            builder.RegisterComponent(_HudController);
            
            // Flow
            builder.RegisterComponent(_FlowController);
        }
        
        private static void Install(IContainerBuilder builder, IInstaller installer)
        {
            installer.Install(builder);
        }
    }
}
