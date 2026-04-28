using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Common.Analytics;
using Common.Analytics.Interfaces;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Persistence.Data;
using Game.Persistence.Interfaces;
using Game.Persistence.Services;
using Game.Store.Data;
using Game.Store.Interfaces;
using Game.Store.Rewards;
using UnityEngine;

namespace Game.IAP.Tests.TestHelpers
{
    internal static class GameIapTestHelpers
    {
        public static IapProduct Product(string key, bool consumable = false)
        {
            var product = ScriptableObject.CreateInstance<IapProduct>();
            SetProperty(product, nameof(IapProduct.ProductKey), key);
            SetProperty(product, nameof(IapProduct.StoreProductId), key);
            SetProperty(product, nameof(IapProduct.IsConsumable), consumable);
            return product;
        }

        public static PurchasableEntry Entry(ItemReward reward, IapProduct product = null)
        {
            var entry = new PurchasableEntry();
            var price = new PriceDefinition();
            SetProperty(price, nameof(PriceDefinition.IapProduct), product);
            SetProperty(entry, nameof(PurchasableEntry.Price), price);
            SetProperty(entry, nameof(PurchasableEntry.Reward), reward);
            return entry;
        }

        public static StoreCatalog Catalog(params PurchasableEntry[] iapEntries)
        {
            var catalog = ScriptableObject.CreateInstance<StoreCatalog>();
            SetProperty(catalog, nameof(StoreCatalog.IapEntries), new List<PurchasableEntry>(iapEntries));
            return catalog;
        }

        public static void SetProperty<T>(object target, string propertyName, T value)
        {
            var field = target.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().Name, propertyName);

            field.SetValue(target, value);
        }

        public static PlayerDataService PlayerDataService(FakePlayerDataStore store = null)
        {
            var service = new PlayerDataService(store ?? new FakePlayerDataStore(), new GameData());
            service.Initialize();
            return service;
        }
    }

    internal sealed class FakeIapService : IIapService
    {
        private readonly HashSet<string> _owned = new HashSet<string>();

        public bool IsPurchaseInProgress { get; set; }
        public int PurchaseCallCount { get; private set; }
        public IapProduct LastPurchasedProduct { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }
        public IapPurchaseResult PurchaseResult { get; set; } = IapPurchaseResult.Purchased("receipt");
        public IReadOnlyList<IapProduct> RestoreResult { get; set; } = Array.Empty<IapProduct>();

        public bool IsOwned(IapProduct product)
        {
            return product != null && _owned.Contains(product.ProductKey);
        }

        public void SetOwned(IapProduct product, bool owned)
        {
            if (owned)
                _owned.Add(product.ProductKey);
            else
                _owned.Remove(product.ProductKey);
        }

        public UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default)
        {
            PurchaseCallCount++;
            LastPurchasedProduct = product;
            LastCancellationToken = ct;
            return UniTask.FromResult(PurchaseResult);
        }

        public UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default)
        {
            return UniTask.FromResult(RestoreResult);
        }

        public UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default)
        {
            return UniTask.FromResult(new IapProductInfo(product, true, IsOwned(product), "$0.99"));
        }
    }

    internal sealed class FakePurchaseService : IPurchaseService
    {
        public bool IsPurchaseInProgress { get; set; }
        public int TryPurchaseCallCount { get; private set; }
        public PurchaseResult Result { get; set; } = PurchaseResult.Succeeded;

        public bool CanPurchase(PurchasableEntry entry) => true;

        public UniTask<PurchaseResult> TryPurchaseAsync(PurchasableEntry entry)
        {
            TryPurchaseCallCount++;
            return UniTask.FromResult(Result);
        }
    }

    internal sealed class FakeGrantContext : IRewardGrantContext
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Add<T>(T service) where T : notnull => _services[typeof(T)] = service;

        public T Resolve<T>() where T : notnull
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException($"Missing service: {typeof(T).Name}");
        }
    }

    internal sealed class FakeReward : ItemReward
    {
        public int GrantCallCount { get; private set; }
        public bool ThrowOnGrant { get; set; }

        public override UniTask GrantAsync(IRewardGrantContext context)
        {
            GrantCallCount++;
            if (ThrowOnGrant)
                throw new InvalidOperationException("grant failed");

            return UniTask.CompletedTask;
        }

        public override string GetDisplayText() => "Fake";
    }

    internal sealed class FakeAnalyticsService : IAnalyticsService
    {
        public readonly List<AnalyticsEvent> Events = new List<AnalyticsEvent>();

        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            Events.Add(analyticsEvent);
        }
    }

    internal sealed class FakePlayerDataStore : IPlayerDataStore
    {
        public PlayerData Data { get; set; } = new PlayerData();
        public int SaveCallCount { get; private set; }

        public PlayerData Load() => Data;

        public void Save(PlayerData data)
        {
            SaveCallCount++;
            Data = data;
        }
    }

    internal sealed class FakeEntitlementStore : IIapEntitlementStore
    {
        public int HasEntitlementCallCount { get; private set; }
        public bool Owned { get; set; }

        public bool HasEntitlement(IapProduct product)
        {
            HasEntitlementCallCount++;
            return Owned;
        }

        public void SetOwned(IapProduct product, bool owned)
        {
            Owned = owned;
        }
    }
}
