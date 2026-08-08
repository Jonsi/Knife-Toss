# Runtime Scene Authority

Last updated: 2026-08-08

## Contract

At runtime there must be exactly one active owner for each of these responsibilities:

- `EventSystem` and `BaseInputModule`;
- `Camera` and `AudioListener`;
- visible `Canvas` and enabled `GraphicRaycaster` sets for the active screen.

Scene-local ownership remains in place. Single-mode scene changes naturally replace the
owner. The MainMenu-to-StagePlay preload is the only additive handoff that temporarily has
two scenes containing authority components, so it uses an explicit suspend/reveal sequence.

## Ownership Map

| Runtime state | Loaded scenes | Authority owner | Notes |
|---|---|---|---|
| Loading | Loading | Loading | Single scene. |
| Main menu starting | MainMenu | MainMenu | MainMenu begins the StagePlay preload. |
| Main menu with preloaded game | MainMenu + StagePlay | MainMenu | `DeferredGameSceneVisibility` disables StagePlay camera, listener, canvases, raycasters, EventSystem, and input module before their normal enable phase. |
| Play handoff | MainMenu + StagePlay | StagePlay | MainMenu authorities are suspended before StagePlay is revealed. MainMenu is then unloaded. |
| Gameplay | StagePlay | StagePlay | Single scene after handoff. Failure, retry, restart, and stage advancement retain this owner. |
| Store overlay | MainMenu + StoreScene | MainMenu | StoreScene has no camera, listener, or EventSystem and uses the MainMenu authority. |
| Stage selection | StagesMenu | StagesMenu | Single scene. |
| Power-ups menu | PowerUpsMenu | PowerUpsMenu | Single scene. |
| PVP | PVPPlay | PVPPlay | Single scene. |
| Return to menu | MainMenu | MainMenu | Single-mode load replaces the previous owner. |

## Handoff Sequence

1. MainMenu calls `DeferredGameSceneVisibility.BeginHiddenLoad()`.
2. StagePlay loads additively.
3. The early-executing `DeferredGameSceneVisibility` gate disables StagePlay authorities
   and presentation before other default-order scene behaviours enable.
4. MainMenu remains the sole interactive and audible scene while StagePlay initializes.
5. On Play, `SceneRuntimeAuthority.Suspend(MainMenu)` disables outgoing scene authorities.
6. StagePlay becomes the active scene.
7. `DeferredGameSceneVisibility.RevealLoadedScene()` restores the components that the gate
   disabled.
8. MainMenu unloads.
9. If reveal or unload throws, the MainMenu suspension restores its previous enabled states.

## Why Execution Order Is Required

StagePlay's EventSystem and AudioListener are serialized enabled for normal direct scene
play. Without early execution, their enable callbacks can run during additive scene load
before the visibility gate disables them, producing a transient duplicate-owner warning.
`DefaultExecutionOrder(-10000)` makes the gate establish hidden ownership first without
requiring the scene assets to be stored in a disabled state.

## Validation Matrix

- [x] MainMenu preload leaves MainMenu as the only active EventSystem/listener/camera.
- [x] Preload produces no duplicate EventSystem or AudioListener warning.
- [x] Play handoff suspends MainMenu before revealing StagePlay.
- [x] Handoff settles to one loaded StagePlay scene and one authority set.
- [x] Handoff Console contains no warnings or errors.
- [x] Store open/close repeated ten times through real UI input.
- [x] MainMenu -> StagesMenu -> MainMenu repeated ten times through real UI input.
- [x] StagePlay failure -> MainMenu completed through the real failure Return button.
- [x] The full tested soak completed with zero Console warnings and zero errors.
- [x] Existing Unity EditMode suite: 42 passed, 0 failed, 0 skipped.
- [ ] PowerUpsMenu and PVP round trips; their MainMenu buttons are not currently wired.
- [ ] Retry, Restart Stage, and next-stage transitions in the current Phase 1 build.

Use Unity MCP for the matrix and record both scene ownership snapshots and Console output.
Do not consider a transition validated only because its final frame contains one owner;
transient warnings and hidden input consumption also fail the contract.
