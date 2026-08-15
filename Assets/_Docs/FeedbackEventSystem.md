# Shared Feedback Event Contract

## Architecture boundary

Existing typed domain events remain authoritative. Power-up outcomes, purchases, rewards,
currency changes, navigation, and level flow are not replaced by the shared feedback event.
A domain-to-feedback adapter may translate those events into `FeedbackEventData` and raise the
shared `FeedbackEventChannel`. Domain services must not reference toast views, audio players,
VFX, haptics, localized copy, sprites, or other presenter assets.

The shared channel fans out immediately. UI, audio, VFX, and haptic presenters subscribe
independently and may use the common rules where arbitration is appropriate. UX-401 defines
the contract and rules only; the reusable toast/callout presenter remains UX-403.

`CueId` is a stable semantic identifier such as `store.insufficient_funds`; it is not display
text. `ContextId` identifies the affected item, transaction, level, or subject when repeated
cues for different subjects must remain distinct. The tuple `Kind:CueId:ContextId` is the
ordinal duplicate key.

## Supported semantics and priority

| Priority | Feedback kinds | Intent |
|---|---|---|
| Critical | Failure, Completion | Terminal run or level outcomes. |
| High | Blocked, InsufficientFunds | The attempted action did not happen and the player needs a reason. |
| Elevated | Purchase, Reward | A durable acquisition or grant occurred. |
| Normal | Success, Consume | Routine positive acknowledgement or resource use. |
| Low | Navigation | Non-critical movement between screens or contexts. |

Priority is derived from `FeedbackKind`; publishers cannot override it. This prevents one
feature from making its own cues disproportionately disruptive.

## Arbitration rules

1. Suppress a request when its duplicate key already matches the active request or any pending
   request. Different contexts remain distinct.
2. Suppress a recently presented duplicate until its per-kind cooldown expires. Cooldown uses
   unscaled time and starts when presentation starts. The exact boundary is accepted.
3. If nothing is active, present immediately.
4. A strictly higher-priority request interrupts the active request. The interrupted request is
   discarded, not resumed, because delayed feedback can describe stale state.
5. Equal- or lower-priority requests queue in descending priority. Requests within the same
   priority are FIFO; equal priority never interrupts.
6. A presenter holds at most eight pending requests. If full, a new request replaces the newest
   member of the lowest-priority tier only when the new request has strictly higher priority;
   otherwise the new request is dropped.
7. Finishing or dismissing the active request presents the head of the ordered queue. Scene or
   presenter teardown clears both active and pending requests.

Cooldowns are 0.15 seconds for Navigation; 0.35 for Success and Consume; 0.75 for Blocked and
InsufficientFunds; 1.0 for Purchase and Reward; and 1.5 for Failure and Completion.

These rules arbitrate presentation only. They never retry an action, mutate inventory or
currency, alter navigation, or change level state.

## Toast/callout presenter (UX-403)

`FeedbackToastPresenter` is the reusable UI subscriber for the shared channel. It lives on a
non-modal, raycast-transparent canvas under the persistent `GameInstaller` and delegates all
arbitration to the presentation-neutral `FeedbackPresentationQueue`. The queue implements the
UX-401 priority, FIFO, interruption, capacity, duplicate, cooldown, and teardown rules without
depending on Unity time or UI components. Active and pending feedback is cleared when the
presenter disables or the active scene changes, so stale copy cannot leak into another screen.

`FeedbackToastCatalog` maps `(FeedbackKind, CueId)` to display copy, accent color, and visible
duration. The presenter has a generic per-kind fallback, but it never displays `CueId` or
`ContextId` as player-facing text. Adding domain-to-feedback adapters remains separate work;
UX-403 does not change domain services or make the toast authoritative for gameplay outcomes.

Focused EditMode coverage validates immediate presentation, strict-priority interruption,
priority/FIFO queueing, full-queue replacement and dropping, active/pending duplicate
suppression, cooldown boundaries, typed-channel subscription, queued copy advancement, scene
clearing, and teardown. Multi-frame evidence is recorded in
`Assets/_Docs/Feedback/Captures/UX403_Toast_Queue.mov`.

## Typed audio, VFX, and haptic outputs (UX-405)

`FeedbackOutputCatalog` is the presentation-owned mapping from an exact ordinal
`(FeedbackKind, CueId)` pair to three optional outputs: an `AudioClip` and volume, a stable VFX
cue identifier, and a light/medium/heavy haptic style. There is deliberately no kind-only or
unknown-cue fallback. A missing catalog, null clip, empty VFX identifier, `None` haptic style,
or unmapped pair is a silent no-op, so adding a domain cue cannot accidentally trigger generic
presentation spam.

`FeedbackAudioPresenter`, `FeedbackVfxPresenter`, and `FeedbackHapticPresenter` subscribe to
the shared channel independently in the persistent `GameInstaller` scope. Each output owns its
own duplicate/cooldown gate, starts idempotently, unsubscribes deterministically, and clears
transient cooldown state on active-scene changes and disposal. Instantaneous outputs are never
queued or retried. A failure in an optional output cannot mutate or repeat the authoritative
action.

Audio reuses the existing persistent `IAudioPlayer`; the existing `AudioListener.pause` mute
path therefore remains authoritative. Haptics use a small adapter over the already-installed
CandyCoded Android/iOS package. UX-406 still owns the future haptic-disable and broader
accessibility policy. `FeedbackVfxSignal` is a presentation-only fan-out seam: scene or
persistent VFX owners subscribe to stable output identifiers and retain ownership of anchors,
prefabs, pooling, and teardown. This avoids putting VFX assets or world positions in domain
events and allows existing pooled stage VFX to remain unchanged until an approved cue maps to
them.

The production catalog currently covers the exact blocked and insufficient-funds cue IDs
emitted by `ActionButtonPresenter` owners. Blocked cues map to the existing `ButtonClickClip` at
0.8 volume, `ui.blocked`, and a light haptic; insufficient-funds cues map to the same neutral UI
clip, `ui.insufficient_funds`, and a medium haptic. The stable VFX identifiers are routed and
tested, but no production VFX asset consumes them yet; final effect art remains outside UX-405.

Focused EditMode coverage validates the production asset wiring, unique exact mappings,
independent audio/VFX/haptic routing, per-context cooldown suppression and exact boundaries,
idempotent subscription, selective teardown, restart cleanup, VFX listener lifecycle, and
null/unmapped safety. Runtime evidence is recorded in
`Assets/_Docs/Feedback/Captures/UX405_Feedback_Output_Hooks.mov`; real-device haptic feel and
platform behavior remain hardware-validation work.
