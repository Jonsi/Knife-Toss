# Phase 1 Plan - Opening Flow Audit

## Phase Goal

Make the first launch and main menu feel focused, with **Play** as the obvious next action and no early interruption from monetization, missions, store, or secondary modes.

The outcome of this phase should be a clearer first impression: the player opens the game, understands where they are, presses Play quickly, and reaches gameplay without friction.

## Success Definition

Phase 1 is complete when:

- A new player can reach gameplay within 2 taps.
- The Play button is visually and functionally the dominant action.
- The current stage is easy to understand.
- Secondary systems are visible only if they do not compete with Play.
- No popup, ad, IAP prompt, mission prompt, or store prompt appears before first gameplay.
- The main menu communicates the current world/stage mood without requiring explanation.

## What Is Needed From You

These are product/design decisions only you should make or approve.

### 1. Confirm The First-Session Philosophy

Choose the intended first impression:

- **Focused arcade:** Play is dominant, everything else is visually quiet.
- **Feature-rich hub:** Store, missions, power-ups, PVP, and skins are all visible immediately.
- **Soft gated hub:** Features are visible but locked/dimmed until introduced.

Recommended choice: **Focused arcade** for the first session.

### 2. Decide Which Buttons Are Prominent At First Launch

Approve the first-launch menu priority.

Recommended priority:

1. Play
2. Current Stage
3. Knife Skin Preview
4. Settings/Mute
5. Stages
6. Missions
7. Store
8. Power-ups
9. PVP
10. Remove Ads

Decision needed:

- Should Store, Missions, Power-ups, PVP, and Remove Ads be fully visible immediately, visually secondary, locked, or hidden until later?

### 3. Decide Whether Feature Gating Is Allowed

Feature gating means a button can become visible or highlighted only after a progression moment.

Decision needed:

- Are we allowed to delay/hide buttons for new players?
- Or should all buttons always exist for testing and navigation?

Recommended:

- Keep all systems accessible for development/debug if needed.
- For player-facing first session, visually delay or soften non-core systems.

### 4. Approve First-Time No-Popup Rule

Decision needed:

- Should the first-time player ever see Privacy, Remove Ads, Daily Missions, Store, or rewarded ad prompts before pressing Play?

Recommended:

- No. The first player action should be Play.

### 5. Define The Main Menu Visual Hierarchy

Decision needed:

- Should the main menu feel like a simple arcade start screen or a full meta hub?

Recommended:

- First session: simple arcade start screen.
- After Stage 1: gradually becomes more of a meta hub.

### 6. Choose Whether Phase 1 Includes Implementation

Decision needed:

- Should Phase 1 only produce audit notes and decisions?
- Or should the agent also make the first menu adjustments?

Recommended:

- Phase 1 should include both audit and small adjustments if the required changes are clear.

## What Is Needed From The Agent

The agent can handle discovery, audit, documentation, and implementation once decisions are clear.

### 1. Inspect Current First-Launch Flow

Agent tasks:

- Review the main menu scene and controller.
- Identify all visible first-menu actions.
- Identify what runs automatically on menu start.
- Check whether any popup can appear before Play.
- Check whether Remove Ads, Missions, Store, Power-ups, or PVP are prominent.
- Check current stage display behavior.
- Check whether selected stage clamping or preload creates any visible delay.

Expected output:

- A short audit summary.
- List of first-launch friction points.
- List of things already working well.

### 2. Map Current Main Menu Button Priority

Agent tasks:

- Locate every main menu button.
- Determine what each button does.
- Classify each as core, secondary, meta, monetization, or settings.
- Compare current priority against recommended priority.

Expected output:

- Button inventory table.
- Priority mismatch list.

### 3. Check First-Time Player Data State

Agent tasks:

- Inspect default player data expectations.
- Confirm what the player owns at first launch.
- Confirm selected/current stage on new data.
- Confirm whether any systems depend on completed stages.

Expected output:

- First-time state summary.
- Any risk that first launch opens on a bad or locked state.

### 4. Recommend Menu Changes

Agent tasks:

- Propose exact changes needed to make Play dominant.
- Identify whether changes are visual, content, progression-gating, or code-driven.
- Separate "must do for Phase 1" from "later polish."

Expected output:

- Recommended Phase 1 change list.
- Estimated risk for each change.

### 5. Implement Approved Small Changes

Possible agent tasks after approval:

- Adjust main menu button visibility or interactability.
- Add first-time gating rules.
- Suppress first-session prompts.
- Improve current stage text clarity.
- Make Play button behavior more direct if needed.
- Add lightweight state flags if required.

Expected output:

- Implemented changes.
- Summary of modified files.
- Verification notes.

### 6. Verify The Flow

Agent tasks:

- Run available checks if feasible.
- Inspect scene/data references where possible.
- Confirm no unrelated user changes were modified.
- Provide a first-session test script for manual Unity playtesting.

Expected output:

- Verification summary.
- Manual QA checklist.
- Any remaining risks.

## Phase 1 Work Sequence

### Step 1 - Product Decision Pass

Owner: You

Decide:

- First-session style: focused arcade, feature-rich hub, or soft gated hub.
- Which buttons should be prominent immediately.
- Whether buttons can be delayed, hidden, dimmed, or locked.
- Whether first-time prompts are banned before gameplay.

Output:

- Approved first-session menu rules.

### Step 2 - Current Flow Audit

Owner: Agent

The agent inspects:

- Main menu scene.
- Main menu controller.
- Player data defaults.
- Scene loading behavior.
- Mission/store/power-up/remove-ads entry points.

Output:

- Opening flow audit summary.

### Step 3 - Gap List

Owner: Agent

The agent compares the current flow against the approved rules.

Output:

- What already matches.
- What conflicts.
- What is unclear.
- What can be fixed safely in Phase 1.

### Step 4 - Implementation Decision

Owner: You

Approve one of:

- **Audit only:** keep findings as planning notes.
- **Light implementation:** make small low-risk UI/flow changes.
- **Full Phase 1 implementation:** add first-time gating/state rules if needed.

Output:

- Chosen implementation scope.

### Step 5 - Implement

Owner: Agent

The agent makes approved changes.

Output:

- Updated files.
- Change summary.

### Step 6 - Manual Playtest

Owner: You, with agent checklist

Test from a fresh player state:

1. Launch game.
2. Observe first visible screen.
3. Press Play.
4. Confirm gameplay starts quickly.
5. Return to menu.
6. Check whether secondary systems feel intrusive.

Output:

- Pass/fail notes.
- Follow-up issues.

## Recommended Phase 1 Decision Defaults

If you want to move quickly, use these defaults:

- First-session style: **Focused arcade**.
- Play: fully prominent.
- Current Stage: visible and clear.
- Knife Skin Preview: visible if already part of menu identity.
- Settings/Mute: visible but small.
- Stages: visible but secondary.
- Missions: hidden or quiet until after Stage 1.
- Store: hidden or quiet until after first reward/currency moment.
- Power-ups: hidden or quiet until first power-up introduction.
- PVP: hidden or quiet until after early progression.
- Remove Ads: hidden or quiet until after first ad exposure or later menu session.
- First-time popups before gameplay: **none**.

## Agent Starting Checklist

When Phase 1 starts, the agent should begin with:

- [ ] Inspect `MainMenu.unity`.
- [ ] Inspect `MainMenuController`.
- [ ] Inspect scene loading flow from Play.
- [ ] Inspect first-time `PlayerData` defaults.
- [ ] Inventory main menu buttons.
- [ ] Check popup/show-on-start behavior.
- [ ] Produce current-vs-target gap list.
- [ ] Wait for implementation scope approval if changes are not obvious.

## Manual QA Checklist

Use this after implementation:

- [ ] Fresh player starts on main menu.
- [ ] Play is the most obvious button.
- [ ] No popup appears before pressing Play.
- [ ] Pressing Play loads the current stage.
- [ ] Current stage text is understandable.
- [ ] Store does not steal attention.
- [ ] Missions do not steal attention.
- [ ] Power-ups do not steal attention.
- [ ] PVP does not steal attention.
- [ ] Remove Ads does not steal attention.
- [ ] Settings/mute remain accessible.

## Phase 1 Exit Criteria

Phase 1 is ready to close when:

- The first-launch menu rules are approved.
- The current menu has been audited against those rules.
- Any high-priority first-flow conflicts are fixed or explicitly deferred.
- A fresh-player manual test confirms the player can reach gameplay quickly without interruption.
