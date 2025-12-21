# MVP/UI Refactor Plan

## Scope
- Establish consistent MVP + R3 + UniTask patterns across UI.
- Keep DI wiring clean and predictable.
- Avoid reintroducing pooling/leak issues in views.

## Plan (Phased)
1. Audit existing UI
   - Inventory all UI prefabs, views, presenters, models, controllers.
   - Mark which ones already follow the pattern and which don’t.
2. Standardize view base usage
   - All popup views inherit `BasePopupView<TModel>`.
   - Non-popup UI gets a dedicated base view if needed.
3. Presenter pattern rollout
   - Use R3 observables from UI elements directly.
   - Keep presenters thin; no redundant one-liner wrappers.
4. Model cleanup
   - Models hold only data required by the view.
   - Presenter gets logic/services via DI (or controlled model actions if legacy).
5. Async UI transitions
   - Implement `Show/Hide` animations with UniTask.
   - Presenters await or fire-and-forget consistently.
6. Pooling policy
   - Keep pooling optional per factory (toggle).
   - Ensure pooled views avoid internal Subjects and OnDestroy cleanup.
7. Testing / validation
   - Presenter unit tests (button clicks -> actions -> close).
   - Manual QA checklist for each popup/hud panel.

## Important Rules
- No Subject fields in views for button clicks; use `OnClickAsObservable()` directly.
- View must be safe for pooling: avoid `Awake` subscriptions that outlive presenter.
- Models are data-only unless explicitly documented as legacy.
- Presenters own subscriptions and dispose them; view should not manage lifecycle.
- DI registrations happen once in the main installer; never re-register the same type.
- Use one DI strategy per dependency (inject OR serialize; never both).
- `Show/Hide` are UniTask and must be consistent across all views.
- Don’t add keys/named DI unless there is no clean type-based alternative.
- Avoid introducing new event channels when services/interfaces are more direct.
- Keep changes localized; don’t refactor unrelated systems in the same pass.

## Architecture Marks
### Folder Structure (Intent)
- `Assets/Scripts/Common/` : Shared MVP core, base patterns, utilities, events.
- `Assets/Scripts/Game/UI/` : UI feature code (Views, Presenters, Models, Factories).
- `Assets/Scripts/Game/UI/Common/` : UI base classes (e.g., `BasePopupView<T>`).
- `Assets/Scripts/Game/UI/Popups/` : Popup-specific views/models.
- `Assets/Scripts/Game/UI/Presenters/` : UI presenters.
- `Assets/Scripts/Game/UI/Factories/` : Popup and UI factories.
- `Assets/Scripts/Game/UI/Controllers/` : UI orchestration/mediators.
- `Assets/Scripts/Game/Installers/` : DI composition root + feature installers.
- `Assets/Prefabs/UI/` : UI prefabs for views.

### Roles
- **View**: Pure UI; exposes R3 observables; async `Show/Hide`.
- **Presenter**: Owns subscriptions; orchestrates UI logic; disposes safely.
- **Model**: Data-only view state (no logic).
- **Factory**: Creates view/presenter; manages pooling.
- **Controller**: Translates game events into UI commands (mediator).
- **Installer**: Registers dependencies exactly once.

## Checklist per UI element
- [ ] View inherits correct base and exposes observables.
- [ ] Model holds only display data.
- [ ] Presenter wires observables and disposes properly.
- [ ] Show/Hide are async and used consistently.
- [ ] Pooling toggle set appropriately for prefab usage.
- [ ] DI wiring verified (no duplicate registrations).
