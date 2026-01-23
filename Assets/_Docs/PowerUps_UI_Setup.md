# Power-Ups UI Implementation Guide (Scene Workflow)

This document explains how to set up the Power-Ups screen as a separate scene in Unity, using a simple list layout where each item displays its own details.

## 1. Scene Creation (PowerUpsMenu)

1.  **Create a New Scene**:
    *   Duplicate `StagesMenu.unity` (easiest way to keep UI consistency) or create a new scene.
    *   Save it as `PowerUpsMenu.unity` in `Assets/Scenes`.
    *   **Important**: Add this scene to the **Build Settings** (`File > Build Settings > Scenes In Build`).

2.  **Setup the Hierarchy**:
    *   Rename the root controller object to `PowerUpsMenuController`.
    *   Remove the `StagesMenuController` component.
    *   Add the `PowerUpsMenuController` component.
    *   Locate the Installer object. Rename it to `PowerUpsMenuInstaller`.
    *   Remove the `StagesMenuInstaller` component and add the `PowerUpsMenuInstaller` component.

3.  **Configure PowerUpsMenuController**:
    *   **Catalog**: Drag the `PowerUpCatalog` asset.
    *   **Item Prefab**: Drag the `PowerUpListItem` prefab (see Section 2).
    *   **List Container**: Assign the Content object of your Scroll View.
    *   **Back Button**: Assign the button to return to Main Menu.

4.  **Configure PowerUpsMenuInstaller**:
    *   **Power Ups Menu Controller**: Drag the GameObject with the `PowerUpsMenuController` component.

## 2. Prefab Creation (PowerUpListItem)

1.  **Create the UI**:
    *   Create a UI panel/card representing a single power-up.
    *   Include: **Icon (Image)**, **Name (Text)**, and **Description (Text)**.
2.  **Add Script**: `PowerUpListItem`.
3.  **Link References**:
    *   **Icon**: Drag the Image component.
    *   **Name Text**: Drag the Name Text component.
    *   **Description Text**: Drag the Description Text component.
    *   **Button**: Drag the Button component (optional, if you want click effects).
4.  **Save as Prefab**: `Assets/Prefabs/UI/PowerUpListItem.prefab`.

## 3. Main Menu Update

1.  **Open MainMenu Scene**: `Assets/Scenes/MainMenu.unity`.
2.  **Select MainMenuController**:
    *   Ensure the **Power Ups Button** is linked.

## 4. Code Changes Summary

*   **PowerUpsMenuController**: Simplified to just spawn items.
*   **PowerUpListItem**: Now handles displaying the Name, Icon, AND Description.
