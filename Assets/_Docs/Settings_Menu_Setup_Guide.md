# Settings Menu Setup Guide

## 1. Asset Creation
1. Create a new ScriptableObject for Privacy Policy:
   - Right-click in Project view -> `Knife Toss/UI/Privacy Policy Content`
   - Name it `PrivacyPolicyContent` (e.g. in `Assets/Configurations/UI/`)
   - Paste your privacy policy text into the `Policy Text` field.

## 2. Prefab/UI Creation (StagePlay Scene)
1. **Settings Popup**:
   - Create a new Panel in your UI Canvas named `SettingsPopup`.
   - Add `SettingsPopup` component.
   - Design the UI with:
     - Sound Toggle (assign to `_SoundToggle`)
     - Privacy Policy Button (assign to `_PrivacyPolicyButton`)
     - Report Error Button (assign to `_ReportErrorButton`)
     - Close Button (assign to `_CloseButton`)
   - Set to inactive by default.

2. **Privacy Policy Popup**:
   - Create a new Panel named `PrivacyPolicyPopup`.
   - Add `PrivacyPolicyPopup` component.
   - Design with:
     - Scroll View (assign `Content` ScrollRect to `_ContentScrollRect`)
     - TextMeshProUGUI inside Scroll View (assign to `_PolicyText`)
     - Close Button (assign to `_CloseButton`)
   - Set to inactive by default.

3. **Report Error Popup**:
   - Create a new Panel named `ReportErrorPopup`.
   - Add `ReportErrorPopup` component.
   - Design with:
     - TMP_InputField (assign to `_ErrorDescriptionInput`)
     - Submit Button (assign to `_SubmitButton`)
     - Cancel Button (assign to `_CancelButton`)
     - Status Text (assign to `_StatusText`)
   - Set to inactive by default.

4. **Settings Button**:
   - Add a Button to your HUD (e.g., top-right corner).
   - Add `SettingsButtonView` component to it.
   - Assign the Button component to the `_Button` field in `SettingsButtonView`.

## 3. Installer Configuration (StageContext)
1. Select the `StageContext` (GameObject with `StageInstaller`).
2. Locate the **UI Popups** header.
3. Drag and drop your new popup references:
   - `Settings Popup` -> `SettingsPopup`
   - `Privacy Popup` -> `PrivacyPolicyPopup`
   - `Report Error Popup` -> `ReportErrorPopup`
4. Locate the **Content & Configs** header.
5. Drag and drop your created ScriptableObject:
   - `Privacy Policy Content` -> `PrivacyPolicyContent`

## 4. Verification
- Run the game.
- Click the Settings button (gear icon) in HUD.
- Verify popups open and close.
- Verify Sound toggle mutes audio.
- Verify Privacy Policy text loads.
- Verify Error Report submits (check persistent data path for file).