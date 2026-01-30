# SRDebugger Integration Plan

## Overview

Integrate SRDebugger into the Knife Toss project using a modular, dependency-injection-friendly approach.
This plan uses `OptionContainer` (recommended for Assembly Definitions) instead of the global `SROptions` partial class.

## 1. Assembly Definition Setup

**Action:** Update `Assets/Scripts/Game/Game.asmdef` to reference `StompyRobot.SRDebugger`.

## 2. Option Containers

Create dedicated classes for different debug categories. These classes will be instantiated and registered with SRDebugger at runtime.

### 2.1 Cheat Options
**File:** `Assets/Scripts/Game/Debug/Options/CheatOptions.cs`

```csharp
using System.ComponentModel;
using Common.Events;
using Game.Flow.Game;
using Game.Level.Interfaces;
using Game.Persistence.Services;
using UnityEngine;

namespace Game.Debug.Options
{
    public class CheatOptions
    {
        private readonly VoidEventChannel _RetryLevelChannel;
        private readonly VoidEventChannel _RestartStageChannel;
        private readonly GameData _GameData;
        private readonly PlayerDataService _PlayerDataService;
        private readonly ILevelCatalog _LevelCatalog;
        
        private int _TargetStageIndex;

        public CheatOptions(
            VoidEventChannel retryChannel,
            VoidEventChannel restartChannel,
            GameData gameData,
            PlayerDataService playerDataService,
            ILevelCatalog levelCatalog)
        {
            _RetryLevelChannel = retryChannel;
            _RestartStageChannel = restartChannel;
            _GameData = gameData;
            _PlayerDataService = playerDataService;
            _LevelCatalog = levelCatalog;
            
            _TargetStageIndex = gameData?.CurrentStageIndex ?? 0;
        }

        [Category("Cheats")]
        [DisplayName("Current Stage")]
        public int CurrentStage
        {
            get => _TargetStageIndex;
            set
            {
                int maxStage = _LevelCatalog?.StageCount - 1 ?? 0;
                _TargetStageIndex = Mathf.Clamp(value, 0, maxStage);
            }
        }
        
        [Category("Cheats")]
        [DisplayName("Go To Stage")]
        public void GoToStage()
        {
            if (_GameData == null) return;
            _GameData.CurrentStageIndex = _TargetStageIndex;
            _RestartStageChannel?.Raise();
            UnityEngine.Debug.Log($"[Cheats] Going to stage {_TargetStageIndex}");
        }
        
        [Category("Cheats")]
        [DisplayName("Restart Level")]
        public void RestartLevel()
        {
            _RetryLevelChannel?.Raise();
        }
        
        [Category("Cheats")]
        [DisplayName("Restart Stage")]
        public void RestartStage()
        {
            _RestartStageChannel?.Raise();
        }
        
        [Category("Cheats")]
        [DisplayName("Add 100 Tomatoes")]
        public void AddTomatoes()
        {
            if (_GameData == null) return;
            _GameData.TotalCollected += 100;
            _PlayerDataService?.RequestSave();
        }
        
        [Category("Cheats")]
        [DisplayName("Clear Progress")]
        public void ClearProgress()
        {
            if (_GameData == null) return;
            _GameData.Reset();
            _PlayerDataService?.ForceSave();
        }
    }
}
```

### 2.2 Player Data Options
**File:** `Assets/Scripts/Game/Debug/Options/PlayerDataOptions.cs`

```csharp
using System.ComponentModel;
using Game.Persistence.Services;
using UnityEngine;

namespace Game.Debug.Options
{
    public class PlayerDataOptions
    {
        private readonly PlayerDataService _PlayerDataService;

        public PlayerDataOptions(PlayerDataService playerDataService)
        {
            _PlayerDataService = playerDataService;
        }
        
        [Category("Player Data")]
        [DisplayName("Highest Stage")]
        public int PlayerHighestStage
        {
            get => _PlayerDataService.GetHighestStageIndex();
            // Note: PlayerDataService might need to expose a setter or direct access to modify this
        }
        
        [Category("Player Data")]
        [DisplayName("Force Save")]
        public void ForceSave()
        {
            _PlayerDataService.ForceSave();
            UnityEngine.Debug.Log("[Player Data] Force saved");
        }

        [Category("Player Data")]
        [DisplayName("Reload Inventory")]
        public void ReloadInventory()
        {
            _PlayerDataService.ReloadPowerUpInventory();
            UnityEngine.Debug.Log("[Player Data] Reloaded inventory");
        }
    }
}
```

## 3. Initialization & Registration

**File:** `Assets/Scripts/Game/Debug/SRDebuggerInitializer.cs`

```csharp
using Game.Debug.Options;
using Game.Persistence.Services;
using Game.Flow.Game;
using Common.Events;
using Game.Level.Interfaces;
using VContainer;
using VContainer.Unity;

namespace Game.Debug
{
    public class SRDebuggerInitializer : IStartable
    {
        private readonly CheatOptions _CheatOptions;
        private readonly PlayerDataOptions _PlayerDataOptions;

        // Dependencies needed to construct the options
        [Inject]
        public SRDebuggerInitializer(
            VoidEventChannel retryChannel,
            VoidEventChannel restartChannel,
            GameData gameData,
            PlayerDataService playerDataService,
            ILevelCatalog levelCatalog)
        {
#if TEST_ENABLED && !DISABLE_SRDEBUGGER
            _CheatOptions = new CheatOptions(retryChannel, restartChannel, gameData, playerDataService, levelCatalog);
            _PlayerDataOptions = new PlayerDataOptions(playerDataService);
#endif
        }

        public void Start()
        {
#if TEST_ENABLED && !DISABLE_SRDEBUGGER
            SRDebug.Init();
            SRDebug.Instance.AddOptionContainer(_CheatOptions);
            SRDebug.Instance.AddOptionContainer(_PlayerDataOptions);
#endif
        }
    }
}
```

## 4. UI Trigger (Toggle Button)

**File:** `Assets/Scripts/Game/Debug/SRDebuggerToggleButton.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace Game.Debug
{
    public class SRDebuggerToggleButton : MonoBehaviour
    {
        [SerializeField] private Button _Button;
        
        private void Awake()
        {
#if !TEST_ENABLED || DISABLE_SRDEBUGGER
            Destroy(gameObject);
#endif
        }

        private void Start()
        {
             _Button.onClick.AddListener(() => {
                 if (SRDebug.Instance.IsDebugPanelVisible)
                     SRDebug.Instance.HideDebugPanel();
                 else
                     SRDebug.Instance.ShowDebugPanel();
             });
        }
    }
}
```

## 5. Installer

**File:** `Assets/Scripts/Game/Installers/DebugInstaller.cs`

```csharp
using Game.Debug;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    public class DebugInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
#if TEST_ENABLED && !DISABLE_SRDEBUGGER
            builder.RegisterEntryPoint<SRDebuggerInitializer>();
#endif
        }
    }
}
```

**Action:** Update `GameInstaller.cs` to call `DebugInstaller`.

```csharp
// Inside GameInstaller.Install
new DebugInstaller().Install(builder);
```

## 6. Implementation Steps

1.  **Dependencies**: Update `Game.asmdef`.
2.  **Code**: Create the classes in `Assets/Scripts/Game/Debug/`.
3.  **Installer**: Create `DebugInstaller.cs` and hook into `GameInstaller.cs`.
4.  **Configuration**: Open SRDebugger Settings window and:
    - Set Loading to `Disabled`.
    - Set Trigger to `Off`.
5.  **UI**: Create a prefab for the toggle button and add it to the Main Menu or HUD.

## Notes
- Ensure `TEST_ENABLED` is defined in Project Settings -> Player -> Scripting Define Symbols for the relevant platforms.
- `PlayerDataService` might need small refactors to fully support all cheat writes (e.g. setters for properties that are currently read-only via the interface).