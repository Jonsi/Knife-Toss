# FMOD Setup Guide

This document explains the FMOD audio system setup for the Knife Toss project.

## Overview

The project uses FMOD Studio for audio playback. The setup follows the [FMOD Unity Integration Guide](https://fmod.com/docs/2.02/unity/user-guide.html).

## Components

### 1. RuntimeManager
- **Location**: Auto-created at runtime
- **Purpose**: Manages FMOD Studio system initialization and lifecycle
- **Setup**: No manual setup required. Created automatically when first accessed.
- **Note**: The RuntimeManager GameObject is marked as `DontDestroyOnLoad` and hidden.

### 2. StudioListener
- **Location**: Main Camera (in MainGame scene)
- **Purpose**: Provides 3D audio positioning for spatial sound effects
- **Setup**: Already configured on the Main Camera
- **Important**: Required for `set3DAttributes()` calls in AudioPlayer to work correctly

### 3. AudioPlayer
- **Location**: `Assets/Scripts/Infrastructure/Audio/AudioPlayer.cs`
- **Purpose**: Plays FMOD events with 3D positioning
- **Features**:
  - Checks FMOD initialization before playing sounds
  - Handles errors gracefully with logging
  - Supports timeline offsets for sound effects
  - Volume control and 3D positioning

### 4. SoundEffect
- **Location**: `Assets/Scripts/Core/Abstractions/SoundEffect.cs`
- **Purpose**: Serializable wrapper for FMOD EventReferences
- **Usage**: Assign EventReferences in the Unity Inspector

## FMOD Settings

### Settings File
- **Location**: `Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset`
- **Configuration**: Managed via Unity Editor menu: **FMOD → Settings**

### Key Settings
- **Automatic Event Loading**: Enabled (loads events automatically)
- **Bank Load Type**: All (loads all banks on startup)
- **Target Asset Path**: `FMODBanks` (where imported banks are stored)

## Using FMOD Events

### Option 1: FMOD Studio Banks (Recommended for Production)
1. Create events in FMOD Studio
2. Build banks from FMOD Studio
3. Import banks into Unity via **FMOD → Settings → Import Banks**
4. Assign EventReferences to `SoundEffect` components

### Option 2: Direct Event References (For Simple Sounds)
1. Create EventReferences directly in Unity Inspector
2. Assign to `SoundEffect` components
3. No bank import required (uses FMOD's default setup)

## Troubleshooting

### No Sound Playing
1. Check Console for FMOD warnings/errors
2. Verify RuntimeManager is initialized (check logs)
3. Ensure StudioListener is on Main Camera
4. Verify EventReferences are assigned correctly
5. Check that banks are loaded (if using FMOD Studio)

### 3D Audio Not Working
1. Ensure StudioListener component is on Main Camera
2. Verify `set3DAttributes()` is being called with correct position
3. Check FMOD event has 3D positioning enabled

### Initialization Errors
- RuntimeManager auto-creates on first access
- If errors occur, check FMOD settings are configured correctly
- Ensure FMOD plugins are properly imported

## Best Practices

1. **Always check initialization**: AudioPlayer checks `RuntimeManager.IsInitialized` before playing
2. **Handle errors gracefully**: All FMOD calls are wrapped in try-catch blocks
3. **Use EventReferences**: Prefer EventReferences over direct bank loading for flexibility
4. **Test in Play Mode**: FMOD requires Play Mode to initialize properly

## References

- [FMOD Unity Integration Guide](https://fmod.com/docs/2.02/unity/user-guide.html)
- [FMOD Studio Documentation](https://fmod.com/docs/2.02/studio/)

