# SRDebugger Project Flow Improvements

This document outlines suggestions for improving the Knife Toss project workflow using SRDebugger's features.

---

## 1. Development Workflow Improvements

### 1.1 Docked Console for Runtime Debugging
**Problem:** Currently, debugging requires switching to Unity Console or attaching debugger.

**Solution:** Use SRDebugger's docked console feature to see logs on-device while playing.

```csharp
// Pin console to game view during development
SRDebug.Instance.DockConsole.IsVisible = true;
SRDebug.Instance.DockConsole.IsExpanded = false; // Collapsed by default
```

**Benefit:** See errors and warnings in real-time on mobile devices without tethering.

---

### 1.2 Docked Profiler for Performance Testing
**Problem:** Performance issues are hard to track on mobile without Unity Profiler connected.

**Solution:** Enable docked profiler for quick FPS and memory checks.

```csharp
// Add to cheats for quick toggle
[Category("Debug")]
[DisplayName("Toggle Profiler")]
public void ToggleProfiler()
{
    SRDebug.Instance.IsProfilerDocked = !SRDebug.Instance.IsProfilerDocked;
}
```

**Benefit:** Identify performance bottlenecks directly on target devices.

---

### 1.3 System Info for Bug Reports
**Problem:** When users report bugs, device information is often missing.

**Solution:** Use SRDebugger's bug reporter with automatic system info capture.

```csharp
// Add custom info to bug reports
SRDebug.Instance.AddSystemInfo(new InfoEntry("Build Version", Application.version), "Game");
SRDebug.Instance.AddSystemInfo(new InfoEntry("Current Stage", _GameData.CurrentStageIndex.ToString()), "Game State");
```

**Benefit:** Every bug report includes device specs, game state, and console logs.

---

## 2. QA Testing Improvements

### 2.1 Stage Skip for Full Playthrough Testing
**Current:** QA must play through every level to test later stages.

**Suggested Cheats:**
```csharp
[Category("QA")]
public void SkipToStage10() => GoToStage(10);

[Category("QA")]  
public void SkipToStage20() => GoToStage(20);

[Category("QA")]
public void SkipToFinalStage() => GoToStage(_LevelCatalog.StageCount - 1);
```

**Benefit:** QA can test any stage immediately without progression.

---

### 2.2 Power-Up Testing Shortcuts
**Current:** Power-ups need to be earned/purchased to test.

**Suggested Cheats:**
```csharp
[Category("QA - Power Ups")]
public void GiveAllPowerUps()
{
    foreach (var config in _PowerUpCatalog.Entries)
    {
        _PowerUpInventory.SetCount(config.InventoryKey, 99);
    }
    _PlayerDataService.SavePowerUpInventory();
}

[Category("QA - Power Ups")]
public void ClearAllPowerUps()
{
    foreach (var config in _PowerUpCatalog.Entries)
    {
        _PowerUpInventory.SetCount(config.InventoryKey, 0);
    }
    _PlayerDataService.SavePowerUpInventory();
}
```

**Benefit:** Test power-up effects without earning them.

---

### 2.3 Difficulty Modifiers
**Suggested Options:**
```csharp
[Category("QA - Difficulty")]
[NumberRange(1, 20)]
public int ExtraKnives { get; set; } = 0;

[Category("QA - Difficulty")]
public bool InfiniteKnives { get; set; } = false;

[Category("QA - Difficulty")]
[NumberRange(0.1f, 3f)]
[Increment(0.1f)]
public float TargetSpeedMultiplier { get; set; } = 1f;
```

**Benefit:** Test edge cases like very fast targets or limited knives.

---

## 3. Analytics & Debugging

### 3.1 Event Channel Monitoring
**Problem:** Hard to know which events fired and when.

**Suggested Implementation:**
```csharp
[Category("Events")]
public string LastEventFired { get; private set; } = "None";

// In event handlers
public void OnLevelCompleted() 
{
    LastEventFired = $"LevelCompleted @ {Time.time:F2}";
    OnPropertyChanged(nameof(LastEventFired));
}
```

**Benefit:** Visible event flow for debugging state machine issues.

---

### 3.2 State Machine Visualization
**Problem:** Current game state is not visible during play.

**Suggested Options:**
```csharp
[Category("State")]
public string GameState => _GameStateMachine?.CurrentState?.GetType().Name ?? "None";

[Category("State")]
public string StageState => _StageStateMachine?.CurrentState?.GetType().Name ?? "None";

[Category("State")]
public string LevelState => _LevelStateMachine?.CurrentState?.GetType().Name ?? "None";
```

**Benefit:** See exactly which state the game is in at any moment.

---

### 3.3 Collision Debug Mode
**Problem:** Hard to see why knife bounced or hit something.

**Suggested Option:**
```csharp
[Category("Debug")]
public bool ShowCollisionGizmos { get; set; } = false;

// Use this flag in collision handlers to visualize impact points
```

**Benefit:** Visual feedback for collision tuning.

---

## 4. Build Configuration

### 4.1 Multi-Environment Support
**Problem:** Same build for all environments.

**Suggested Approach:**
```csharp
// In SROptions
[Category("Environment")]
public string CurrentEnvironment => 
#if PRODUCTION
    "Production"
#elif STAGING
    "Staging"
#else
    "Development"
#endif
;
```

**Configuration in ProjectSettings:**
- Development: `TEST_ENABLED;DOTWEEN`
- Staging: `TEST_ENABLED;DOTWEEN;STAGING`
- Production: `DOTWEEN;DISABLE_SRDEBUGGER;PRODUCTION`

**Benefit:** Clear environment identification, automatic SRDebugger removal in production.

---

### 4.2 Build Version Display
**Suggested Addition:**
```csharp
[Category("Build Info")]
public string Version => Application.version;

[Category("Build Info")]
public string BuildNumber => Application.buildGUID;

[Category("Build Info")]
public string UnityVersion => Application.unityVersion;
```

**Benefit:** Always know which build is running during testing.

---

## 5. Remote Config Testing

### 5.1 Config Override Options
**Problem:** Testing different remote config values requires server changes.

**Suggested Approach:**
```csharp
[Category("Remote Config")]
public bool UseLocalOverrides { get; set; } = false;

[Category("Remote Config")]
[NumberRange(1, 10)]
public int LocalKnifeCount { get; set; } = 5;

// In game code
int knifeCount = SROptions.Current.UseLocalOverrides 
    ? SROptions.Current.LocalKnifeCount 
    : _RemoteConfig.GetInt("knife_count");
```

**Benefit:** Test different config values without changing server.

---

## 6. User Testing Sessions

### 6.1 Session Recording Trigger
**Suggested Implementation:**
```csharp
[Category("Recording")]
public void StartRecording()
{
    // Integration with your analytics or recording service
    Debug.Log("[Session] Recording started");
}

[Category("Recording")]
public void StopRecording()
{
    Debug.Log("[Session] Recording stopped");
}
```

**Benefit:** Mark important moments during user testing sessions.

---

### 6.2 Quick Bug Report
**Suggested Implementation:**
```csharp
[Category("Support")]
public void QuickBugReport()
{
    string description = $"Stage: {_GameData.CurrentStageIndex}, Time: {Time.time:F0}s";
    SRDebug.Instance.ShowBugReportSheet(descriptionContent: description);
}
```

**Benefit:** One-tap bug report with auto-filled context.

---

## 7. Pinned Options for Quick Access

### Recommended Pins for Development:
1. Current Stage (editable)
2. FPS display
3. Restart Level button
4. Toggle Console

### Recommended Pins for QA:
1. Skip to Next Stage
2. Give All Power-Ups
3. Current State display
4. Quick Bug Report

```csharp
// Auto-pin recommended options on first run
[RuntimeInitializeOnLoadMethod]
static void SetupDefaultPins()
{
#if TEST_ENABLED && !DISABLE_SRDEBUGGER
    SRDebug.Instance.PinOption("Current Stage");
    SRDebug.Instance.PinOption("Restart Level");
#endif
}
```

---

## 8. Keyboard Shortcuts (Editor/Desktop)

### Suggested Additional Shortcuts:
| Shortcut | Action |
|----------|--------|
| F5 | Restart Level |
| F6 | Restart Stage |
| F7 | Previous Stage |
| F8 | Next Stage |
| F9 | Toggle Profiler |
| F10 | Quick Bug Report |

**Implementation:** Create `DebugKeyboardShortcuts.cs` MonoBehaviour.

---

## 9. Integration with Existing Tools

### 9.1 Replace Editor CheatsWindow
**Current:** `CheatsWindow.cs` only works in Editor during Play Mode.

**Recommendation:** Deprecate editor cheats in favor of SRDebugger options.
- Same functionality, works on device
- No need to maintain two systems
- Consistent UX across platforms

### 9.2 Complement PlayerDataWindow
**Current:** `PlayerDataWindow.cs` works on file data outside play mode.

**Recommendation:** Keep both:
- Editor window for offline data editing
- SRDebugger for runtime data editing
- Consider adding "Sync with Runtime" button to editor window

---

## 10. Production Safety

### 10.1 Automatic Disabling
**Build Script Integration:**
```csharp
// PreBuild script
[PostProcessBuild]
public static void OnPreBuild(BuildTarget target, string path)
{
    if (IsProductionBuild())
    {
        SRDebugEditor.SetEnabled(false);
    }
}
```

### 10.2 Entry Code Protection
**For staging builds that reach external testers:**
1. Enable "Require Entry Code" in SRDebugger settings
2. Set a 4-digit PIN
3. Share PIN only with QA team

---

## Summary Priority List

### High Priority (Implement First)
1. Cheats tab with stage navigation
2. Player data viewing/editing
3. Docked console toggle
4. Build version display

### Medium Priority (Implement Second)
5. Power-up testing shortcuts
6. State machine visualization
7. Quick bug report
8. Keyboard shortcuts

### Low Priority (Nice to Have)
9. Event monitoring
10. Collision debug mode
11. Remote config overrides
12. Session recording markers

---

## Next Steps

1. Review this document and select which features to implement
2. Prioritize based on current pain points
3. Implement in phases, testing each addition
4. Document usage for team members
