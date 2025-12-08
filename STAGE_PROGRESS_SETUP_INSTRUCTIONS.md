# Stage Progress Bar Setup Instructions

## Overview
The Stage Progress Bar displays a horizontal chain showing: **Current Stage** → **Level Progress** → **Next Stage**

Visual layout: `[Current Stage] ─ [Level 1] [Level 2] [Level 3] ─ [Next Stage]`

Where:
- **Stage circles** = Large wooden circles with stage numbers (serialized, not prefabs)
- **Level circles** = Small circles (green = completed/current, brown = not reached) - instantiated from prefab
- **Connecting line** = Single Image that stretches between stage circles

---

## Step 1: Create Level Circle Prefab

1. Create a new GameObject in your scene (temporary, for prefab creation)
2. Add `LevelCircleView` component to it
3. Add an `Image` component to the same GameObject (for the base wooden circle)
   - Assign the sprite: `Assets/Art/icons/stages/level_complete_icon(1).png` (base, without green)
4. Add a child GameObject named "GreenFill" with an `Image` component
   - Assign the green fill sprite to this Image
   - Set the Image to be a child of the base circle
5. In the `LevelCircleView` component:
   - Assign the base Image to `Base Circle Image` field
   - Assign "GreenFill" Image to `Green Fill Image` field
6. Drag this GameObject to your Prefabs folder to create the prefab
7. Delete the temporary GameObject from the scene

**Prefab Structure:**
```
LevelCirclePrefab
├── LevelCircleView (component)
├── Image (component - base wooden circle sprite)
└── GreenFill (Image component - green fill sprite)
```

---

## Step 2: Set Up Main Progress Bar Structure

1. Create a root GameObject named `StageProgressBar` in your HUD Canvas
2. Add `StageProgressBarView` component to it
3. Add `HorizontalLayoutGroup` component to it:
   - Set `Child Control Width` = **true**
   - Set `Child Control Height` = **true**
   - Set `Child Force Expand Width` = **false**
   - Set `Child Force Expand Height` = **false**
   - Set `Spacing` = 0
   - Set `Child Alignment` = Middle Center

4. Create child GameObjects in this order:
   - `CurrentStageCircle` (first child)
   - `ConnectingLine` (second child)
   - `LevelContainer` (third child)
   - `ConnectingLine` (fourth child - duplicate of second)
   - `LastStageCircle` (fifth child)

**Hierarchy:**
```
StageProgressBar
├── CurrentStageCircle
├── ConnectingLine (first)
├── LevelContainer
├── ConnectingLine (second)
└── LastStageCircle
```

---

## Step 3: Configure CurrentStageCircle

1. Select `CurrentStageCircle`
2. Add `StageCircleView` component
3. Add an `Image` component (for the big circle sprite)
4. Add a child GameObject with `TextMeshProUGUI` component (for stage number)
5. In `StageCircleView`:
   - Assign the Image to `Circle Image` field
   - Assign the TextMeshProUGUI to `Stage Number Text` field
6. Set the RectTransform size to match your big circle sprite (e.g., 100x100)
7. **Important:** Add `LayoutElement` component:
   - Set `Flexible Width` = 0
   - Set `Preferred Width` = 100 (or your circle size)
   - Set `Min Width` = 100

---

## Step 4: Configure LastStageCircle

1. Select `LastStageCircle`
2. Add `StageCircleView` component
3. Add an `Image` component (for the big circle sprite)
4. Add a child GameObject with `TextMeshProUGUI` component (for stage number)
5. In `StageCircleView`:
   - Assign the Image to `Circle Image` field
   - Assign the TextMeshProUGUI to `Stage Number Text` field
6. Set the RectTransform size to match your big circle sprite (e.g., 100x100)
7. **Important:** Add `LayoutElement` component:
   - Set `Flexible Width` = 0
   - Set `Preferred Width` = 100 (or your circle size)
   - Set `Min Width` = 100

---

## Step 5: Configure ConnectingLine (Both Instances)

1. Select the first `ConnectingLine`
2. Add an `Image` component
   - Assign sprite: `Assets/Art/icons/stages/connecting_line(1).png`
   - Set Image Type = **Sliced** (9-slice) or **Tiled** depending on your sprite
3. **Important:** Add `LayoutElement` component:
   - Set `Flexible Width` = 0
   - Set `Preferred Width` = 30 (or desired line width)
   - Set `Min Width` = 30
4. Repeat for the second `ConnectingLine`

---

## Step 6: Configure LevelContainer

1. Select `LevelContainer`
2. Add `HorizontalLayoutGroup` component:
   - Set `Child Control Width` = **false**
   - Set `Child Control Height` = **false**
   - Set `Child Force Expand Width` = **false**
   - Set `Child Force Expand Height` = **false**
   - Set `Spacing` = 0 (or small value for gaps between levels)
   - Set `Child Alignment` = Middle Center
3. **Critical:** Add `LayoutElement` component:
   - Set `Flexible Width` = **1** (this makes it stretch!)
   - Set `Preferred Width` = 0
   - Set `Min Width` = 0

This makes LevelContainer stretch between the two stage circles while levels inside keep their natural size.

---

## Step 7: Configure StageProgressBarView Component

In the `StageProgressBarView` component on the root GameObject, assign:

**Stage Circles:**
- `Current Stage Circle` → CurrentStageCircle GameObject
- `Last Stage Circle` → LastStageCircle GameObject

**Level Container:**
- `Level Container` → LevelContainer GameObject

**Prefabs:**
- `Level Circle Prefab` → Your LevelCirclePrefab

**Connecting Line:**
- `Connecting Line` → Either ConnectingLine GameObject (both use the same Image)

---

## Layout Behavior

- **CurrentStageCircle**: Fixed size at left (LayoutElement prevents stretching)
- **ConnectingLine (first)**: Fixed width between current stage and levels
- **LevelContainer**: **Stretches** to fill space (LayoutElement flexibleWidth = 1)
  - Levels inside keep their natural size (HorizontalLayoutGroup doesn't control width)
- **ConnectingLine (second)**: Fixed width between levels and last stage
- **LastStageCircle**: Fixed size at right (LayoutElement prevents stretching)

This ensures levels are evenly distributed across the available space while maintaining their scale.

---

## Testing

1. Assign all references in `StageProgressBarView`
2. The script will automatically:
   - Update stage numbers on CurrentStageCircle and LastStageCircle
   - Instantiate level circles in LevelContainer
   - Set filled/empty states based on current level
3. Test with different numbers of levels to ensure proper stretching
