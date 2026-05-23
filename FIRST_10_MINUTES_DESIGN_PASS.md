# Toss Madness - First 10 Minutes Design Pass

## Goal

The first 10 minutes should make the player understand the game, feel skilled quickly, experience a first failure without quitting, earn something visible, and see a reason to return.

This pass focuses on product experience only. It is not about architecture or technical setup.

## Design Intent

The opening experience should teach through play, not explanation. The player should be throwing knives within seconds, then gradually discover the surrounding systems once the core action is already fun.

The intended player journey:

1. Start the game.
2. Immediately understand "tap to throw knives into the target."
3. Complete a very easy first level.
4. Learn that stuck knives become obstacles.
5. Fail once in a fair, readable way.
6. Retry and succeed.
7. Complete the first stage.
8. Receive a clear reward.
9. See that stages, skins, power-ups, and missions exist.
10. Choose a next goal.

## First 10 Minutes Timeline

### 0:00-0:15 - Arrival

The player lands on the main menu.

What they should notice:

- Big Play action.
- Current stage displayed.
- Knife skin visible.
- Background hints at the active world.
- Other buttons exist, but Play feels like the obvious action.

Recommended behavior:

- New players should be guided toward Play.
- Store, missions, power-ups, and PVP can be visible, but should not compete with Play.
- Avoid showing popups before the first play session.

Product risk:

- If the player sees too many systems before throwing a knife, the game feels heavier than it is.

### 0:15-1:00 - First Level

The first level should be nearly impossible to misunderstand.

Recommended setup:

- One large target.
- Slow constant rotation.
- No pre-stuck knives.
- No moving target.
- Generous knife count.
- Low required hit count.
- Optional collectible placed in a safe, easy-to-hit position.

Player lesson:

- Tap throws a knife.
- Knives stick into the target.
- Enough hits break the target.

Success target:

- Most players should clear this level on the first attempt.

### 1:00-2:00 - First Obstacle

The second level introduces the main danger: existing knives.

Recommended setup:

- Same target readability.
- Slightly faster rotation.
- One or two stuck knives after the player has already landed hits, or one simple pre-stuck knife if the visual is very clear.
- Keep required hit count modest.

Player lesson:

- Do not hit existing knives.
- Timing matters.

Success target:

- Some players may fail, but the failure should look obviously fair.

### 2:00-3:30 - First Failure And Retry

The first failure is a key product moment. It should create "I know what I did wrong," not confusion.

Recommended fail popup priority:

- Retry should be the most obvious action.
- Restart stage should be available but secondary.
- Continue should not be pushed aggressively during the first failure.

Player lesson:

- Failure is recoverable.
- Retry is fast.
- The game respects short attempts.

Success target:

- The player retries within a few seconds.

### 3:30-5:00 - First Stage Completion

The first stage should end with a satisfying payoff.

Recommended reward moment:

- Show a clear reward popup.
- Give soft currency.
- If possible, show progress toward the first skin or power-up purchase.
- Use a strong target break/shatter moment before the reward.

Player lesson:

- Completing a stage gives rewards.
- Progression continues.

Success target:

- The player understands what they earned and what unlocked next.

### 5:00-7:00 - First Meta Choice

After the first stage, the player returns to the menu or stage map with a new choice.

Recommended surfacing:

- Highlight the next stage.
- Optionally introduce the stage selection screen.
- If the player earned enough currency for nothing yet, show the store later.
- If the player earned a power-up, introduce the power-up menu or HUD at the next relevant moment.

Player lesson:

- The game has a progression map.
- The next action is clear.

Success target:

- The player starts Stage 2 without feeling lost.

### 7:00-10:00 - First System Reveal

Only after the core loop is understood should the game introduce a meta system.

Best first reveals:

- Daily missions, if the player has already made progress naturally.
- Store, if the player has enough currency or is close to the first purchase.
- Power-ups, if the next stage is tuned to benefit from one.

Avoid revealing all systems at once.

Recommended order:

1. Stage progression.
2. Rewards/currency.
3. Missions.
4. Store/skins.
5. Power-ups.
6. Ads/continues.
7. PVP.

## Recommended Unlock And Introduction Order

### Available Immediately

- Play.
- Current stage display.
- Basic knife skin selector if at least one skin is owned.
- Settings/mute.

### After Stage 1 Completion

- Stage selection screen.
- First visible reward popup.
- Daily missions button, with existing progress if possible.

### After Stage 2 Or First Meaningful Currency Earn

- Store.
- Knife skin purchase goal.
- Watch-ad-for-currency CTA, if monetization is being tested.

### After First Failure Or Stage 2 Completion

- Power-ups.
- Give or showcase one starter power-up.
- Explain through use, not a long text panel.

### Later

- PVP.
- Remove Ads.
- Interstitial-style ad exposure.

## Stage 1 Tuning Target

Stage 1 should teach, reward, and create confidence.

Recommended structure:

| Level | Purpose | Difficulty |
| --- | --- | --- |
| 1 | Learn tap-to-throw and target break | Very easy |
| 2 | Learn timing and stuck-knife danger | Easy |
| 3 | Learn direction change or faster spin | Easy-medium |
| 4 | Learn collectibles as optional reward | Easy-medium |
| 5 | Mini test that combines the above | Medium |

Recommended rules:

- No confusing movement in Stage 1.
- No tiny targets.
- No heavy pre-stuck knife pattern.
- Keep the first level almost guaranteed.
- Put the first "real" fail chance after the player has already succeeded at least once.

## Stage 2 Tuning Target

Stage 2 should feel like "same game, sharper timing."

Possible additions:

- Faster rotation.
- Direction changes.
- More collectible temptation.
- One or two pre-stuck knives.
- Slightly tighter knife counts.

Avoid:

- Moving targets plus fast rotation too early.
- Multiple targets before the player is comfortable.
- Power-up dependency.

## Stage 3 Tuning Target

Stage 3 can introduce movement and more expressive target behavior.

Possible additions:

- Pulsating spin.
- Sinusoidal movement.
- Multi-target levels.
- More meaningful use of power-ups.

Player goal:

- The player starts to feel that the game has depth beyond simple tap timing.

## Reward Pacing

The first 10 minutes should contain at least three positive reward signals:

- A small in-level collectible.
- A stage completion reward.
- Mission progress or mission claim opportunity.

Recommended first rewards:

- Soft currency after Stage 1.
- A visible mission progress update after completing early levels.
- A starter power-up by the end of Stage 2 or Stage 3.

Avoid:

- Giving too many currencies at once.
- Showing a store full of unaffordable items too early.
- Reward popups that do not clearly say what changed.

## Store Timing

The store should be introduced when the player has a reason to care.

Good moments:

- The player has enough currency for the cheapest skin.
- The player is close to affording the cheapest skin.
- The player has used a power-up and understands why buying more could help.
- The player has watched or declined a rewarded ad.

Bad moments:

- Before the first gameplay session.
- Before the player has earned any currency.
- Immediately after a frustrating fail.

## Mission Timing

Missions are best introduced after the player has already made progress toward them.

Example:

- Player completes Stage 1.
- Mission button pulses lightly.
- Opening missions shows "Complete 3 levels" already partially progressed.

This creates a feeling of discovery and reward, not homework.

## Power-Up Timing

Power-ups should be introduced after the player understands normal play.

Recommended intro:

- Give one starter power-up.
- Present it as a rescue or special move.
- Put the player in a level where it is useful but not mandatory.

Best first power-up:

- Chrono Shift is likely the cleanest first tutorial power-up because slowing the target reinforces the core timing mechanic.

Good second power-up:

- Shatter Strike, because it creates a dramatic "break the target now" moment.

## Ads And Continue Timing

Ads should not appear before the player has formed a positive opinion of the core game.

Recommended:

- No forced/interstitial ad in the first 10 minutes.
- Continue ad can appear after a failure, but should be framed as optional help.
- Store rewarded ad can appear once the player understands currency.
- Remove Ads should be visible later, not pushed immediately.

Risk:

- Early ad pressure can make the game feel extractive before it feels fun.

## First-Time UX Checklist

Use this as a pass/fail checklist.

- [ ] New player can start gameplay within 2 taps.
- [ ] No monetization popup appears before first gameplay.
- [ ] First level is clear without text instructions.
- [ ] First successful hit has strong feedback.
- [ ] First failed hit is visually understandable.
- [ ] Retry after failure is fast and obvious.
- [ ] Stage 1 completion gives a clear reward.
- [ ] Player sees what stage is next.
- [ ] Player sees at least one long-term goal after Stage 1.
- [ ] Store is not introduced before the player has currency context.
- [ ] Missions are introduced only when they can show meaningful progress.
- [ ] Power-ups are introduced after basic skill is established.
- [ ] Ads are optional and framed as help or bonus.
- [ ] PVP does not compete with the main progression in the opening.

## Content Checklist

- [ ] Stage 1 Level 1: simple target, slow spin, no obstacles.
- [ ] Stage 1 Level 2: simple timing challenge.
- [ ] Stage 1 Level 3: slightly faster or reversed spin.
- [ ] Stage 1 Level 4: collectible temptation.
- [ ] Stage 1 Level 5: combined mini test.
- [ ] Stage 2: introduces sharper timing and limited obstacles.
- [ ] Stage 3: introduces first movement behavior.
- [ ] First stage reward is satisfying and clear.
- [ ] First skin price is reachable within a reasonable early-session window.
- [ ] First power-up grant occurs before difficulty spikes.

## Product Copy Principles

Keep text short and action-focused.

Good:

- "Tap to throw"
- "Avoid your knives"
- "Stage clear"
- "Claim"
- "Retry"
- "Continue"

Avoid:

- Long tutorial paragraphs.
- Explaining every system before use.
- Technical terms like "objective type" or "configuration."

## Suggested First 10 Minutes Target Metrics

These are product goals to validate with playtesting.

- 90%+ of new players start the first level.
- 80%+ complete Level 1 on first try.
- 70%+ retry after their first failure.
- 60%+ complete Stage 1.
- 50%+ start Stage 2.
- 40%+ open at least one meta screen after Stage 1.
- Less than 10% see an ad before understanding the reward/continue value.

## Immediate Next Sprint

Recommended next sprint focus:

1. Tune Stage 1 as a tutorial-quality stage.
2. Define the first three world names and their mechanical identities.
3. Make the Stage 1 completion reward presentation crisp.
4. Decide exactly when Store, Missions, Power-ups, PVP, and Remove Ads first become prominent.
5. Add one starter power-up moment after the player understands normal throwing.

The goal is not to add more systems. The goal is to make the existing systems unfold in the right order.

## Work Phases

These phases are designed to be completed separately. Each phase should leave the game in a better, shippable state even if the next phase has not started yet.

### Phase 1 - Opening Flow Audit

**Goal:** Make the first launch and main menu feel focused, with Play as the obvious next action.

**Why this matters:** If the player is overwhelmed before the first throw, all later systems lose value. This phase makes sure the game starts with confidence.

**Scope:**

- Review the current main menu layout from a first-time-player perspective.
- Decide which buttons are prominent immediately and which should be visually secondary.
- Confirm Play starts the current unlocked stage quickly.
- Confirm no popup or monetization prompt interrupts the first session.
- Confirm the current stage display is understandable.

**Deliverables:**

- Main menu first-time UX notes.
- Decision on which features are visible immediately.
- Decision on which features are hidden, locked, dimmed, or delayed.
- Small UI/content adjustments if needed.

**Acceptance criteria:**

- A new player can start gameplay within 2 taps.
- Play is the dominant action.
- Store, Missions, Power-ups, PVP, and Remove Ads do not compete with Play.
- No ad, IAP, or mission popup appears before first gameplay.

**Dependencies:** None.

### Phase 2 - Stage 1 Tutorial Tuning

**Goal:** Turn Stage 1 into a clean learn-by-playing tutorial stage.

**Why this matters:** Stage 1 teaches the entire core game. It should be easy, readable, satisfying, and fair.

**Scope:**

- Tune Stage 1 level-by-level.
- Ensure Level 1 is extremely simple.
- Introduce stuck-knife danger only after the player has succeeded once.
- Introduce collectibles as optional rewards.
- Avoid moving targets, tiny targets, heavy obstacle patterns, and confusing behavior.
- Make the final level of Stage 1 a small combined test.

**Deliverables:**

- Updated Stage 1 level design.
- Notes describing what each Stage 1 level teaches.
- First-pass difficulty target for each level.

**Acceptance criteria:**

- Level 1 teaches tap-to-throw and target breaking without text-heavy instruction.
- Level 2 or Level 3 teaches stuck-knife danger clearly.
- At least one level includes an easy collectible opportunity.
- Stage 1 can be completed by most new players.
- The first likely failure feels fair and understandable.

**Dependencies:** Phase 1 is recommended but not required.

### Phase 3 - Failure, Retry, And Continue Experience

**Goal:** Make the first failure feel recoverable instead of punishing.

**Why this matters:** The first failure is where many casual players decide whether the game is fair. The player should immediately know what happened and what to do next.

**Scope:**

- Review the fail popup.
- Make Retry the clearest action.
- Keep Restart Stage available but secondary.
- Decide when Continue should appear in the first-time experience.
- Decide whether first continue is free, ad-based, currency-based, or delayed.
- Make sure failure feedback is visually obvious.

**Deliverables:**

- Failure flow decision.
- Continue timing decision.
- Popup priority pass.
- Any needed copy adjustments.

**Acceptance criteria:**

- Retry is the obvious first action after failure.
- The player is not pushed into an ad too early.
- The player understands why they failed.
- Restart and Continue are present only when they help the decision.

**Dependencies:** Best after Phase 2, because failure tuning depends on Stage 1 difficulty.

### Phase 4 - Stage 1 Completion Reward

**Goal:** Make the first completed stage feel rewarding and forward-moving.

**Why this matters:** Completing Stage 1 is the first proof that the game has progression. The reward moment should tell the player "you did something meaningful."

**Scope:**

- Define the exact Stage 1 reward.
- Review the reward popup experience.
- Make sure the reward is visually clear.
- Show what changed: currency gained, stage unlocked, mission progress, or next goal.
- Decide whether the player returns to main menu, stage map, or continues directly.

**Deliverables:**

- Stage 1 reward definition.
- Reward presentation notes.
- Next-action decision after Stage 1 completion.

**Acceptance criteria:**

- The player clearly sees what they earned.
- The player understands that Stage 2 is now the next goal.
- The reward is useful enough to matter.
- The moment feels positive, not like an interruption.

**Dependencies:** Phase 2.

### Phase 5 - Early Unlock Order

**Goal:** Decide when Missions, Store, Power-ups, PVP, and Remove Ads become prominent.

**Why this matters:** The game already has many systems. This phase prevents the first 10 minutes from feeling crowded.

**Scope:**

- Define feature visibility across the first few stages.
- Decide which systems are immediately visible, delayed, highlighted, locked, or introduced contextually.
- Decide whether Store appears after Stage 1 or later.
- Decide when Missions first appear.
- Decide when Power-ups are introduced.
- Decide when PVP and Remove Ads become prominent.

**Deliverables:**

- Unlock/introduction table.
- Main menu visibility rules.
- First-time system reveal sequence.

**Acceptance criteria:**

- The player is never introduced to more than one new meta system at a time.
- Store appears only after currency has meaning.
- Missions appear when they can show progress.
- Power-ups appear after the player understands normal throws.
- PVP and Remove Ads do not distract from early progression.

**Dependencies:** Phase 1 and Phase 4 recommended.

### Phase 6 - First Mission Moment

**Goal:** Make daily missions feel like a reward discovery, not homework.

**Why this matters:** Missions are strongest when the player opens them and sees progress already earned.

**Scope:**

- Choose which mission types are best for the first session.
- Decide if the mission button should highlight after Stage 1.
- Confirm "Complete 3 levels" or similar early mission has natural progress.
- Review mission reward clarity.
- Avoid showing missions before the player has done anything.

**Deliverables:**

- First-session mission reveal rule.
- Recommended first mission pool or weighting.
- Mission popup copy/clarity notes.

**Acceptance criteria:**

- Opening missions after Stage 1 shows meaningful progress.
- At least one mission feels achievable soon.
- Rewards are understandable.
- The mission UI does not block core progression.

**Dependencies:** Phase 4 and Phase 5.

### Phase 7 - First Store Moment

**Goal:** Introduce the store only when the player understands currency and has a desirable goal.

**Why this matters:** Store visits should feel motivated. The player should know what they can buy, why it matters, and how to earn more.

**Scope:**

- Pick the first recommended purchase target.
- Confirm cheapest skin price and early currency earnings feel aligned.
- Decide whether the player should be able to afford a first skin quickly or only see progress toward it.
- Review store card clarity, ownership state, and price readability.
- Decide when the watch-ad-for-currency CTA first appears.

**Deliverables:**

- First purchase target.
- Early currency pacing recommendation.
- Store intro timing.
- Watch-ad CTA timing.

**Acceptance criteria:**

- The player understands what currency buys.
- The cheapest skin or first store goal feels reachable.
- Store does not feel empty, unaffordable, or premature.
- Watch-ad currency is framed as optional.

**Dependencies:** Phase 4 and Phase 5.

### Phase 8 - First Power-Up Moment

**Goal:** Introduce one power-up through a useful gameplay moment.

**Why this matters:** Power-ups should feel like exciting tools, not unexplained inventory.

**Scope:**

- Choose the first introduced power-up.
- Recommended first power-up: Chrono Shift.
- Decide whether the player receives one starter unit.
- Create or tune a level where the power-up is useful but not required.
- Make the use result highly visible.

**Deliverables:**

- First power-up choice.
- Starter grant decision.
- Level/use-case recommendation.
- Power-up intro copy, if needed.

**Acceptance criteria:**

- The player understands what the power-up did.
- The power-up helps with a real timing challenge.
- The level remains possible without it.
- The player has a reason to want more later.

**Dependencies:** Phase 2 and Phase 5.

### Phase 9 - World Identity And Campaign Spine

**Goal:** Give the early campaign a clear identity and difficulty direction.

**Why this matters:** Worlds should make progression feel bigger than a list of numbered stages.

**Scope:**

- Name the first three worlds.
- Define the mechanical theme of each world.
- Define the visual/emotional theme of each world.
- Decide what each world teaches.
- Align Stage 1, Stage 2, and Stage 3 with that progression.

**Deliverables:**

- World naming list.
- World mechanical identity table.
- First three world progression notes.

**Acceptance criteria:**

- Each world has a distinct product identity.
- The player can describe why the next world feels different.
- Difficulty progression is intentional.
- Visual theme and gameplay theme support each other.

**Dependencies:** Can run in parallel with Phases 2-8.

### Phase 10 - First 10 Minutes Playtest Pass

**Goal:** Validate the full opening experience as a player journey.

**Why this matters:** Individual pieces can be good while the full sequence still feels confusing. This phase checks the actual flow.

**Scope:**

- Play from fresh data.
- Track time to first throw.
- Track first failure.
- Track Stage 1 completion.
- Track first meta screen opened.
- Note confusion, friction, boredom, or pressure.
- Compare against target metrics.

**Deliverables:**

- Playtest notes.
- Fix list ordered by impact.
- Updated target metrics if needed.

**Acceptance criteria:**

- First throw happens quickly.
- Stage 1 teaches the core loop.
- Failure is understandable.
- Stage completion feels rewarding.
- The next action after Stage 1 is clear.
- The player is not overloaded by systems.

**Dependencies:** Best after Phases 1-8. Phase 9 can be ongoing.

## Suggested Execution Order

1. Phase 1 - Opening Flow Audit.
2. Phase 2 - Stage 1 Tutorial Tuning.
3. Phase 3 - Failure, Retry, And Continue Experience.
4. Phase 4 - Stage 1 Completion Reward.
5. Phase 5 - Early Unlock Order.
6. Phase 6 - First Mission Moment.
7. Phase 7 - First Store Moment.
8. Phase 8 - First Power-Up Moment.
9. Phase 9 - World Identity And Campaign Spine.
10. Phase 10 - First 10 Minutes Playtest Pass.

Phases 6, 7, and 8 can be swapped depending on which meta system you want to prioritize first. Phase 9 can happen at any time because it is mostly product direction and content naming.
