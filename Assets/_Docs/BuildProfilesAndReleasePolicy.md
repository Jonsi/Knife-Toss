# Build Profiles and Release Policy

Last updated: 2026-08-08

## Profiles

Android and iOS each have three explicit Build Profile assets:

| Flavor | Development Build | Profile defines | Intended use |
|---|---:|---|---|
| Development | Yes | `TEST_ENABLED` | Local iteration, cheats, mock services, SRDebugger, profiler connection. |
| QA | Yes | `TEST_ENABLED`, `QA_BUILD` | Distributable internal QA without automatic profiler connection. |
| Production | No | `PRODUCTION_BUILD`, `DISABLE_SRDEBUGGER` | Store/release candidate with test tooling excluded. |

`TEST_ENABLED` is no longer a global Android or iOS scripting symbol. It belongs only to
the Development and QA profiles, preventing a generic mobile build from inheriting test
tooling accidentally.

## Enforced Rules

`ProductionBuildPolicy` runs before every player build.

- Every non-development build must explicitly contain `PRODUCTION_BUILD`.
- Non-development builds cannot contain `TEST_ENABLED`.
- Production cannot contain `QA_BUILD` or use Development Build.
- Production must contain `DISABLE_SRDEBUGGER`.
- QA must use Development Build and contain `TEST_ENABLED`.
- Production profile activation stages debug Resources and mock source directories outside
  Unity's import graph; Development/QA activation restores them with their original GUIDs.
- Production builds fail if any development-only content directory is still enabled.

An invalid combination throws `BuildFailedException` before a player artifact is produced.
The current Editor configuration can also be checked from:

`Knife Toss > Build > Validate Active Build Policy`

## Runtime Service Policy

| Service | Development / QA | Production fallback |
|---|---|---|
| Ads | `MockAdProvider` and interactive mock popup | `UnavailableAdProvider`; ads remain unavailable until a real SDK adapter is integrated. |
| IAP | Stub base provider with `MockIapProvider` test override | `StubIapProvider`; purchases fail safely until a real store adapter is integrated. |
| Analytics | Console-backed `MockAnalyticsService` | Silent `NullAnalyticsService` until a production backend is integrated. |
| Debug UI and cheats | Compiled under `TEST_ENABLED` | Custom Debug assembly excluded; SRDebugger disabled. |

Mock ad, mock IAP, mock analytics, cheats, and SRDebugger content are both compile-gated and
physically staged out while a Production profile is active. Production fallbacks never
simulate a completed ad, purchase, reward, or analytics delivery.

## Known Release Dependencies

The production profiles are safe fallbacks, not monetization-complete releases. Before a
commercial build:

- integrate the chosen production ad SDK through `IAdProvider`;
- integrate the store provider through `IIapProvider`;
- integrate the analytics backend through `IAnalyticsService`;
- decide production Firebase/Remote Config behavior for device and simulator profiles;
- perform purchase, ad, consent, offline, and restore testing on signed device builds.
- confirm whether the current Android identity `com.CuzGames.TossMdaness` and label
  `Toss Mdaness` are intentional before any store upload;
- triage the Android build's shader warning volume, currently dominated by Sentis variants,
  and fix the compressed application icon warning.

## Validation Performed

- [x] Normal Standalone development compilation with `TEST_ENABLED`.
- [x] Deliberate Standalone non-test compilation with `TEST_ENABLED` temporarily removed.
- [x] Development policy combination accepted.
- [x] QA policy combination accepted.
- [x] Production policy combination accepted.
- [x] Unsafe non-development/test combinations rejected.
- [x] Production without `DISABLE_SRDEBUGGER` rejected.
- [x] Existing Unity EditMode suite: 42 passed, 0 failed, 0 skipped.
- [ ] Full Android Development build.
- [ ] Full Android QA build.
- [x] Full non-development Android Production APK build: 0 errors.
- [x] Android APK contains no `TEST_ENABLED`, mock ad/IAP/analytics, cheat-tab, or SRDebugger
  names in IL2CPP metadata, native symbols, or serialized player data.
- [x] Android manifest does not declare the application debuggable.
- [ ] Full iOS Development build.
- [ ] Full iOS QA build.
- [ ] Full iOS Production Xcode archive and artifact scan.

Android production artifact evidence: 66 MB APK, SHA-256
`aa979eaa86a118c907238f152ee18c6e57253d4b795de68258b5ea6cdea678ce`.
