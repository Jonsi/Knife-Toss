# Architecture Plan (Early Stage)

## Goals
- Stabilize architecture for future scalability (features, monetization, analytics).
- Introduce a lean MVP foundation with clear separation of concerns.
- Add generic factory + pooling patterns for gameplay entities and UI views.
- Reduce tight coupling between MonoBehaviours and services.

## Current Snapshot (Observed)
- VContainer DI with multiple installers.
- Pooling exists for knives/particles; targets and UI instantiate directly.
- Views (MonoBehaviours) contain significant logic (e.g., knife/target).
- UI logic is mixed in views without a presenter layer.

## Target Architecture Layers
- Common: cross-feature services and shared abstractions (MVP core, pooling, factories, event buses).
- Domain: pure data and rules (no Unity dependencies).
- Application: services and use-cases (game flow, power-ups, analytics).
- Presentation: MVP (Views are thin; Presenters hold logic and subscriptions).
- Infrastructure: analytics, monetization, persistence adapters.

## MVP Foundation (Required Shape)
```csharp
// ==========================================
// 1. CORE INTERFACES
// ==========================================
public interface IModel { }

public interface IView<TModel> where TModel : IModel {
    void Initialize(TModel model);
    void Show();
    void Hide();
}

public interface IFactory<out TPresenter, in TView, in TModel>
    where TView : MonoBehaviour
    where TModel : IModel
{
    TPresenter Create(TView prefab, TModel model, Transform parent);
}

public interface IPresenter<out TView, out TModel> : IDisposable
    where TView : IView<TModel>
    where TModel : IModel
{
    TView View { get; }
    TModel Model { get; }
}

public interface IPooledView<TModel> : IView<TModel>
    where TModel : IModel
{
    void OnSpawn();
    void OnDespawn();
}

// ==========================================
// 2. THE LEAN BASE PRESENTER
// ==========================================
public abstract class BasePresenter<TView, TModel> : IPresenter<TView, TModel>
    where TView : class, IView<TModel>
    where TModel : IModel
{
    public TView View { get; }
    public TModel Model { get; }

    protected BasePresenter(TView view, TModel model) {
        View = view;
        Model = model;
    }

    // Single point of cleanup.
    // Subclasses override this to unbind events.
    public abstract void Dispose();
}

// ==========================================
// 3. VIEW POOL PROVIDER
// ==========================================
using UnityEngine;
using UnityEngine.Pool;

public class ViewPoolProvider<TView, TModel>
    where TView : MonoBehaviour, IPooledView<TModel>
    where TModel : IModel
{
    private readonly TView _prefab;
    private readonly Transform _root;
    private readonly ObjectPool<TView> _pool;

    public ViewPoolProvider(TView prefab, Transform root, int defaultCapacity = 10, int maxCapacity = 50) {
        _prefab = prefab;
        _root = root;

        _pool = new ObjectPool<TView>(
            createFunc: CreateInstance,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyInstance,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxCapacity
        );
    }

    private TView CreateInstance() => Object.Instantiate(_prefab, _root);

    private void OnGet(TView view) {
        view.gameObject.SetActive(true);
        view.OnSpawn();
    }

    private void OnRelease(TView view) {
        view.OnDespawn();
        view.gameObject.SetActive(false);
    }

    private void OnDestroyInstance(TView view) => Object.Destroy(view.gameObject);

    public TView Get() => _pool.Get();
    public void Release(TView view) => _pool.Release(view);
}

// ==========================================
// 4. BASE MVP FACTORY (SCRIPTABLE OBJECT + POOL + DI)
// ==========================================
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using System;

public abstract class BaseMVPFactorySO<TPresenter, TView, TModel> : ScriptableObject
    where TPresenter : IPresenter
    where TView : MonoBehaviour, IPooledView<TModel>
    where TModel : IModel
{
    [SerializeField] protected TView prefab;
    [SerializeField] private int defaultPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;

    private ObjectPool<TView> _pool;
    private Transform _poolRoot;

    // We pass the IObjectResolver so the factory can resolve Presenter dependencies
    public virtual TPresenter Create(TModel model, Transform parent, IObjectResolver container)
    {
        if (_pool == null) InitializePool(parent);

        TView viewInstance = _pool.Get();

        // Use VContainer to instantiate the Presenter.
        // This automatically injects any extra services (Sound, Analytics, etc.)
        // while we manually provide the View, Model, and Release action.
        return container.Instantiate<TPresenter>(new object[]
        {
            viewInstance,
            model,
            (Action<TView>)_pool.Release
        });
    }

    private void InitializePool(Transform parent)
    {
        _poolRoot = parent;
        _pool = new ObjectPool<TView>(
            createFunc: () => Instantiate(prefab, _poolRoot),
            actionOnGet: v => { v.gameObject.SetActive(true); v.OnSpawn(); },
            actionOnRelease: v => { v.OnDespawn(); v.gameObject.SetActive(false); },
            actionOnDestroy: v => Destroy(v.gameObject),
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
    }
}

// Example usage (reference only):
// [CreateAssetMenu(fileName = "KnifeFactory", menuName = "MVP/Factories/Knife")]
// public class KnifeFactorySO : BaseMVPFactorySO<KnifePresenter, KnifeView, KnifeModel> { }

// ==========================================
// 5. UI NAVIGATION SERVICE (WINDOW MANAGER)
// ==========================================
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public interface IUINavigationService {
    void OpenPopup<TP, TV, TM>(BaseMVPFactorySO<TP, TV, TM> factory, TM model, Transform parent)
        where TP : IPresenter where TV : MonoBehaviour, IPooledView<TM> where TM : IModel;
    void CloseTop();
}

public class UINavigationService : IUINavigationService {
    private readonly IObjectResolver _container;
    private readonly Stack<IPresenter> _activePresenters = new();

    public UINavigationService(IObjectResolver container) {
        _container = container;
    }

    public void OpenPopup<TP, TV, TM>(BaseMVPFactorySO<TP, TV, TM> factory, TM model, Transform parent)
        where TP : IPresenter where TV : MonoBehaviour, IPooledView<TM> where TM : IModel
    {
        TP presenter = factory.Create(model, parent, _container);
        _activePresenters.Push(presenter);
    }

    public void CloseTop() {
        if (_activePresenters.Count == 0) return;

        IPresenter top = _activePresenters.Pop();
        top.Dispose();
    }
}
```

## Incremental Migration Plan

### Phase 0: Documentation + Baseline
- Create this plan doc and keep it updated.
- Identify the highest-risk coupling points and runtime dependencies.

### Phase 1: MVP Core + UI Pilot
- Add MVP core interfaces and `BasePresenter` in `Assets/Scripts/Common/MVP`.
- Add a minimal UI presenter (start with `StageFailPopup`).
  - View: only UI references and view events.
  - Presenter: subscribes to events and calls channels/services.
- Add `UINavigationService` (window manager) as a singleton to manage popup stack.
- Update `UIInstaller` to register presenter + view.

### Phase 2: UI View Pooling
- Add a generic `ViewPool<TView>` and `ViewFactory<TView>`.
- Convert HUD elements or popups to pooled views (if reused often).

### Phase 3: Entity Factory + Pooling
- Introduce pooled `TargetFactory` and a generic entity pool.
- Convert `TargetFactory` to use pool provider instead of `Instantiate`.
- Add a presenter or model wrapper for target if behavior grows.

### Phase 4: Knife MVP Layer
- Wrap `KnifeView` with `IKnifeView` interface and a presenter.
- Gradually move non-visual logic out of `KnifeView` into presenter/model.

### Phase 5: PowerUp Extensibility
- Replace switch logic with `IPowerUpHandler` strategies.
- Allow analytics/monetization hooks per power-up.

## Suggested Folder Structure
```
Assets/Scripts/Common
  /MVP
  /Pooling
  /Factories
  /Events
Assets/Scripts/Game
  /Core
  /Domain
  /Application
  /Presentation
    /Views
    /Presenters
  /Infrastructure
  /Pooling
  /Factories
```

## Risks + Mitigations
- Risk: Over-refactor early. Mitigation: apply MVP to one UI feature first.
- Risk: Unity dependencies leak into Domain. Mitigation: keep models pure.
- Risk: Pooling bugs (inactive objects). Mitigation: add OnSpawned/OnDespawned.

## Next Actions (Concrete)
1) Add MVP core types (IModel, IView<TModel>, BasePresenter).
2) Convert `StageFailPopup` to MVP and register in `UIInstaller`.
3) Introduce generic view pooling for UI elements.
4) Convert `TargetFactory` to use pooling.
5) Add `IPowerUpHandler` strategy pattern.

## Open Decisions
- Popup management rules (stacking, blocking, queueing, transitions).
- `UINavigationService` API surface (close by type, close all, replace top).
- Whether to use MVP for gameplay entities or keep for UI only initially.
