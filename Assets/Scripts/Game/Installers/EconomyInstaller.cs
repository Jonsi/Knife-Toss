using Common.Economy.Data;
using Common.Economy.Interfaces;
using Common.Economy.Services;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Economy.Services;
using Game.Persistence.Services;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Registers economy services: currency value resolution and balance management.
    /// </summary>
    public class EconomyInstaller : IInstaller
    {
        private readonly CurrencyTypeCatalog _Catalog;
        private readonly CurrencyBalanceChangedEventChannel _BalanceChangedChannel;

        /// <summary>
        /// Initializes a new instance of the EconomyInstaller.
        /// </summary>
        /// <param name="catalog">Catalog of currency types and default values.</param>
        /// <param name="balanceChangedChannel">Channel raised when a currency balance changes.</param>
        public EconomyInstaller(
            CurrencyTypeCatalog catalog,
            CurrencyBalanceChangedEventChannel balanceChangedChannel)
        {
            _Catalog = catalog;
            _BalanceChangedChannel = balanceChangedChannel;
        }

        /// <summary>
        /// Registers economy services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Catalog).AsSelf();
            builder.Register<CurrencyValueResolver>(Lifetime.Singleton)
                .WithParameter(_Catalog)
                .As<ICurrencyValueProvider>();

            builder.Register<CurrencyManager>(Lifetime.Singleton)
                .As<ICurrencyService>();

            builder.RegisterInstance(_BalanceChangedChannel).AsSelf();
        }
    }
}
