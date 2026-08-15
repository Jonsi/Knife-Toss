# Project Agent Rules

## Git Commits

- Never create a Git commit without the user's separate, explicit approval to commit.
- General authorization to begin or continue work—including messages such as "go," "start," or "proceed"—does not authorize a commit.
- After completing and validating changes, leave them uncommitted until the user explicitly asks for or approves the commit.

## Motion and Time-Based Validation

- Whenever validation depends on behavior across multiple frames—such as animations, VFX, transitions, cooldowns, shader motion, or other time-based effects—record a short screen video and inspect it.
- Still screenshots and state queries may supplement the recording, but they do not replace it for multi-frame behavior.

## Unity Prefab Architecture

- Before authoring a reusable UI element, search the project for an existing prefab, view, binding, or shared component that already owns the same responsibility.
- Repeated UI elements—including price pills, status pills, card shells, CTA rows, icon sockets, and offer layouts—must be shared prefab assets or nested prefabs. Do not reconstruct near-identical GameObject hierarchies independently in scenes, card prefabs, or editor builders.
- Compose card and screen prefabs from nested prefabs. Use prefab variants only for coherent, reusable variations; keep item-specific content and runtime state in view bindings or data.
- Editor builders may create or update prefab assets and wire references, but they must instantiate existing reusable prefabs with `PrefabUtility.InstantiatePrefab` instead of rebuilding those elements with `new GameObject` and manual component construction.
- Separate prefab reuse from runtime pooling: prefab assets provide a shared source of truth, while pools or virtualized collections bound the number of live instances. Use a bounded pool or virtualization when a runtime collection can grow substantially; do not pre-author one hierarchy object per catalog item.
- Multiple prefab instances appearing in the Hierarchy are expected. Verify that they retain prefab links and share the intended source asset; duplicated instances are not the same as duplicated authoring sources.
- Before delivery, inspect repeated objects for prefab linkage, confirm that shared edits propagate from the source prefab, and report the collection's intended scaling behavior.
