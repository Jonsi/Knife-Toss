# Gameplay Playtest Harness

`GameplayPlaytestHarness` is an Editor-only control and telemetry surface for repeatable
playtesting through Unity MCP. It does not invoke UI callbacks or gameplay methods. Every
action queues ordinary mouse state into Unity's Input System, so the game's normal input,
UI, collision, and progression paths remain responsible for the result.

Available operations:

- `SnapshotJson()` records the active scene, target motion, hit count, stuck-knife angles,
  visible buttons, and pending input.
- `ClickButton(name, visibleText)` sends a real press/release at a matching UI button.
- `TapGameplay(delaySeconds)` sends an ordinary gameplay tap.
- `ScheduleSafeHit()` predicts a clear target lane from live transforms and throw speed,
  then schedules a real tap.
- `ScheduleIntentionalCollision()` watches live motion and sends a real tap timed to hit
  an existing knife, allowing the failure/restart path to be tested reliably.
- `Cancel()` releases any held pointer and clears pending work.

The timing helpers must be run with the target Unity Editor in the foreground. When the
Editor is throttled in the background, large frame steps can make fast knives tunnel past
2D colliders and invalidate both human and automated timing observations.

The same safe-hit, intentional-collision, cancel, and snapshot actions are available from
`Tools > Knife Toss > Playtest` for manual validation.
