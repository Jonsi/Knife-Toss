using KnifeToss.Application;
using KnifeToss.Core.Abstractions;
using KnifeToss.Infrastructure.Audio;
using KnifeToss.Infrastructure.Input;
using KnifeToss.Infrastructure.Services;
using VContainer;
using VContainer.Unity;

public class ProjectLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register core services that should be available project-wide
        builder.Register<IGameEventBus, GameEventBus>(Lifetime.Singleton);
        
        // Register RuntimeCacheService for efficient Unity object access
        builder.Register<RuntimeCacheService>(Lifetime.Singleton);
        
        // Register audio player - using AudioPlayer (creates its own GameObject)
        // If you need UnityAudioPlayer from hierarchy, uncomment the line below and remove the AudioPlayer registration
        builder.Register<IAudioPlayer, AudioPlayer>(Lifetime.Singleton);
        
        // Register input service - must be from hierarchy (MonoBehaviour)
        // Note: InputService GameObject must exist in the scene for this to work
        builder.RegisterComponentInHierarchy<IInputService>();
    }
}
