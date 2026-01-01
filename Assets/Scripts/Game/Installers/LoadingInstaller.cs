using Game.Flow.Loading;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the Loading scene only.
    /// </summary>
    public class LoadingInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LoadingController>();
        }
    }
}
