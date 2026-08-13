# MainMenu Modern Forest Arcade evidence

Date: 2026-08-12
Unity: 6000.4.2f1, Android target
Scene: `Assets/Scenes/MainMenu.unity`

## Finished composition

The real MainMenu now uses one Modern Forest Arcade hierarchy inside `SafeAreaRoot`. The retained
stage background, Toss Madness logo, particle presentation, and controller/service ownership are
unchanged. The default knife preview and throwing stump are newly authored production art; all 12
unlockable knife skins remain unchanged. Newly produced chrome uses the existing approved
blank surfaces from `Assets/Art/UI/Core` plus seven bespoke illustrated sprites from
`Assets/Art/UI/Icons`. Their source sheet, ImageGen output ID, prompt, hashes, alpha-removal chain,
and deterministic normalization script are retained under `ArtSource/UI/Icons/ModernForestArcade`.
Hero-art sources and their separate deterministic build are retained under
`ArtSource/GameContent/ModernForestArcade`. No Layer Lab, legacy navigation, legacy hero, or
reference-board pixels are used by the newly produced MainMenu visuals.

Reference-canvas geometry is exact at 1080×1920 (3 px/dp):

| Element | Anchor | Anchored position | Size | Contract |
|---|---|---:|---:|---|
| Settings | top-left | `108,-108` | `144×144` (48 dp) | illustrated Settings gear on Icon Socket |
| Store | top-left | `264,-108` | `144×144` (48 dp) | illustrated market stall on Icon Socket |
| Missions | top-left | `420,-108` | `144×144` (48 dp) | illustrated checklist on Icon Socket |
| Currency | top-right | `-246,-108` | `420×144` (140×48 dp) | Status surface, live TMP value and illustrated coin |
| Remove Ads | top-right | `-108,-324` | `144×144` (48 dp) | conditional Icon Socket; approved crossed-`ADS` mark |
| Logo | top-center | `0,-345` | `773.68×490.03` | retained brand art |
| Selector | center | `0,-10` | `780×600` | production default knife / target; service-selected skins remain live |
| Previous / Next | selector center | `-315,53` / `315,53` | `144×144` each (48 dp) | illustrated wooden arrows on Icon Sockets |
| Stage chip | bottom-center | `0,515` | `480×120` (160×40 dp) | Price surface and controller-owned live TMP text |
| Play | bottom-center | `0,335` | `780×168` (260×56 dp) | Primary; unscaled hit root / scaled presentation child |
| Stages | bottom-center | `0,148` | `720×144` (240×48 dp) | Secondary; unscaled hit root / scaled presentation child |

The three top-left sockets use a 12 px / 4 dp grid gap. SafeAreaRoot continues to apply all device
edges. The background remains outside the safe root and fills the entire Canvas, so insets do not
create gaps or change the existing non-preserved-aspect background behavior.

## Live state and behavior owners

| Visible value/action | Authoritative owner retained |
|---|---|
| Play, Stages, Store routes and navigation order | `MainMenuController` / `SceneLoaderService` |
| duplicate submission, loading, blocked feedback, restoration | `ActionButtonPresenter` plus `MainMenuController` |
| Settings action | `SettingsButtonView` / scene-owned settings presenter |
| Missions action | `MainMenuController` / `DailyMissionPopupController` |
| Remove Ads visibility, purchase availability and result | `MainMenuRemoveAdsFlow`; no fake price or availability |
| currency value and updates | `CurrencyBalanceDisplay` / currency service |
| current Stage text and clamping | `MainMenuController`, `GameData`, progression catalog/services |
| knife options, equipped selection and wrapping | `KnifeSkinService` / `KnifeSkinSelectorView` |

`KnifeSkinSelectorView` now hides both arrows when fewer than two selectable options exist and
sets interactability before presentation visibility. Multiple options retain the service-owned
wrapping/equip behavior. Navigation locking disables the selector without inventing local state.

## Transition presentation

Menu elements retain `UiElementAnimator`, now normalized to the shared short semantic timing:
top controls enter first, logo second, selector third, stage information next, then Primary and
Secondary actions. Movement is restrained; no looping control animation was added. The knife
preview keeps a 10 px / 1.4 s unscaled float. All menu and selector tweens keep the existing
disable/destroy cancellation paths. Navigation timing and route authority were not changed.

The encoded motion evidence is `Captures/MainMenu_Transitions.mov`: H.264 High,
yuv420p limited-range BT.709, 540×960, constant 30 fps, 120 frames, 4.000 s, 489,622 bytes.
Unity was sampled on wall clock for 4.000470 s at 30 Hz; the per-frame capture times are retained
in `MainMenu_Transitions_Timing.csv`. The timeline is not synthetic: it records an invoked entrance,
restrained knife float, invoked exit at approximately 1.50 s, and restored entrance at approximately
2.25 s. `MainMenu_Transitions_ContactSheet.png` was decoded from the final MOV and visually
inspected; it shows complete exit and restoration with no stuck alpha, scale, or position state.

The hero-art-specific motion evidence is `Captures/MainMenu_KnifeFloat_v1.mov`: H.264 High,
yuv420p, 1080×1920, constant 30 fps, 126 frames, and 4.200 s. This is a wall-clock capture of the
live Unity Game View, not a synthetic timeline. Eight frames decoded from the encoded movie into
`MainMenu_KnifeFloat_v1_ContactSheet.png` show the production knife completing its restrained
10 px / 1.4 s vertical float while the stump, controls, and layout stay stable.

## Runtime and layout evidence

| File | Condition |
|---|---|
| `Captures/MainMenu_Standard_1080x1920.png` | actual MainMenu; Stage 1, currency 103, one knife option, Remove Ads available |
| `Captures/MainMenu_ProgressedHigh_Multiple_1080x1920.png` | temporary display-only Stage 999 / 9,999,999 / multiple-option arrows / Remove Ads unavailable |
| `Captures/MainMenu_Standard_360x640.png` | Lanczos phone-size derivative of the exact standard capture |
| `Captures/MainMenu_Standard_Grayscale.png` | grayscale derivative of the exact standard capture |
| `Captures/MainMenu_TallSafe_1080x2160.png` | Unity Android 1080×2160 fixed preset plus nonpersistent 24/72/24/120 px safe insets |
| `Captures/MainMenu_TabletSafe_1440x2560.png` | Unity Android 1440×2560 fixed portrait preset plus nonpersistent 72/72/72/96 px safe insets |
| `Captures/MainMenu_Transitions.mov` | wall-clock transition and selector-motion recording |
| `Captures/MainMenu_Transitions_ContactSheet.png` | sixteen frames decoded from the final MOV |
| `Captures/MainMenu_IllustratedIcons_Standard.png` | actual MainMenu with the five normally visible illustrated icons |
| `Captures/MainMenu_IllustratedIcons_Selector.png` | temporary Play Mode visibility proof for illustrated Previous/Next arrows |
| `Captures/MainMenu_IllustratedIcons_v2.png` | actual MainMenu after the review-only Remove Ads and Currency revisions |
| `Captures/MainMenu_RemoveAds_v3.png` | actual MainMenu with the clean ring-free crossed-video Remove Ads glyph |
| `Captures/MainMenu_FinalReviewed_v4.png` | final live 1080×1920 review with the proportion-correct frameless Remove Ads glyph |
| `Captures/MainMenu_FinalReviewed_v4_360x640.png` | phone-size derivative inspected for hierarchy and small-scale legibility |
| `Captures/MainMenu_FinalReviewed_v4_Grayscale.png` | grayscale derivative inspected for silhouette and value hierarchy |
| `Captures/MainMenu_RemoveAds_ADS_Approved_v5.png` | live 1080×1920 proof of the user-approved large `ADS` mark with red-orange slash on top |
| `Captures/MainMenu_KnifeTarget_v1.png` | actual MainMenu with the production default knife and throwing stump |
| `Captures/MainMenu_KnifeFloat_v1.mov` | wall-clock live Game View proof of the production knife float |
| `Captures/MainMenu_KnifeFloat_v1_ContactSheet.png` | eight frames decoded from the final encoded hero-art movie |

The temporary runtime injections changed only scene TMP copy, selector display flags, active
visibility, and SafeAreaRoot anchors in Play Mode. They did not call progression, currency, IAP,
save, equip, or navigation services. Teardown restored Stage 1, currency 103, one-option arrows
hidden, Remove Ads visibility, full SafeAreaRoot anchors, and a non-dirty scene.

SHA-256:

| File | SHA-256 |
|---|---|
| `ModernForestArcade_IconFamily_Chroma_v1.png` | `63c456496891fba3b34b5d460e3eaf5923ef4e626aa212d250551df0f0320580` |
| `ModernForestArcade_IconFamily_RGBA_v1.png` | `ed9c15c73eccb8c295166eac91e24c2f9dada13da17ed51a49b2b5d7931f9c4c` |
| `ModernForestArcade_IconFamily_Chroma_v2.png` | `abf0bbaf2518ca00d6dc5c29a232d1e6688089710f1cba1843faa31710fb0d3e` |
| `ModernForestArcade_IconFamily_RGBA_v2.png` | `90cea9b814400ac98e85750f629f0a40be859bcccb6feb5ac9ddcfdea3d3e147` |
| `IconRemoveAds.png` | `2bc5ca8f51f4607fa58fe5d9506ccf06e9db6a9ab025d0f1692684dcbb99bc17` |
| `IconCurrency.png` | `9d8a8b68a133a4c96db1d4ed7e0e5e44b3278a758de308cb3a8f9737b742350a` |
| `MainMenuIcons.spriteatlasv2` | `c58d4359fa70f74ce414ad797f608615b537537b9bd5ea37a8680900a6e1c018` |
| `MainMenu_IllustratedIcons_Standard.png` | `f7db2bd61b83789baffa078f8544e622c0638475482ef9b9853b2370f8cd3f8c` |
| `MainMenu_IllustratedIcons_Selector.png` | `555bf448910d4532fa37d8914545713c52c2981505313a85f7e58270850fece4` |
| `MainMenu_IllustratedIcons_v2.png` | `92dec0e52b6293b6a7a34ec28783913c5f202b64c35d70683cfeb67d62a5293f` |
| `IconRemoveAds.png` (v3) | `ad1febd32e3569602c6d7407dea7352a72094fd2763b15ae7b0629cbafdbbe81` |
| `MainMenu_RemoveAds_v3.png` | `a2b8a4760ae2cecc0c7d82d2085654e35a4f9adfa8cd571605fd3c442ed1828a` |
| `IconRemoveAds_Chroma_v4.png` | `ae5bdbe632c7afea6483c003ea94314af20a0ffb077d33df65d622e3f6323936` |
| `IconRemoveAds_RGBA_v4.png` | `a37bf48f092d4431caf65f4ec0926ace4dcad9ffa53ce0b0a589d6b7fef24ee2` |
| `IconRemoveAds.png` (v4) | `07b6a611c8084e9a60698a5d8d3964046f2eff832c112ef35655e8f5b8e43383` |
| `MainMenu_FinalReviewed_v4.png` | `7a813b0443a9ebd8a3487aeb91a2d4c23cbac13cb3f473511a4882f40e36dc92` |
| `MainMenu_FinalReviewed_v4_360x640.png` | `62bb09daf3e86b90c3b743293c4c8585fd51f7e23ca850d2e3370392b67dad45` |
| `MainMenu_FinalReviewed_v4_Grayscale.png` | `5c56dd9c26ef5497a63a4cfc8dd95b55b3d364bdcfe9c8e95c41f4e2ce4985b7` |
| `IconRemoveAds_Chroma_v5.png` | `99d7d79b5dfdc4431ea370db07580c2bd750462f50eea3affe00125673b324ba` |
| `IconRemoveAds_RGBA_v5.png` | `072203accac8c7bc132c8e875f70b30efb4c1294740f42ca6d828b78262a6241` |
| `IconRemoveAds.png` (approved v5) | `b878dd66f8fd59bba3440025b21cfeb6b2174a434a21d4ccd986be573e169c98` |
| `MainMenu_RemoveAds_ADS_Approved_v5.png` | `b4b1c33a7cfbb36d2c599dbf3709e3ebd7d8a8b613e9512574edcfe5b8620120` |
| `DefaultKnife.png` | `59729620fd316faedf1fa427f65c9f389f664e97329a8a747653858ffd6b8be5` |
| `MainMenuTarget.png` | `09ceef719da7579da4667eaa0b0a7be37fa64ba8ad2e5264cf687b34c13345c2` |
| `MainMenu_KnifeTarget_v1.png` | `b6d54ea3232a156341a92e337af09fa4faff2a55ab70151d6b62ad81aabaac42` |
| `MainMenu_KnifeFloat_v1.mov` | `eaba80104366b4146ae375f544437140c60bab989a0b0e4bfbd818ab1845e65c` |
| `MainMenu_KnifeFloat_v1_ContactSheet.png` | `0ec183a57c3babc04eece41ccc8df8d8f694d677b2f3d04adca29509ff84cbdf` |
| `MainMenu_Standard_1080x1920.png` | `1919d2661fadf2792d9e46eedaf39884d81f271fe04d297bc86916a184240af1` |
| `MainMenu_ProgressedHigh_Multiple_1080x1920.png` | `775332aeee16f0c6387d6a8ee6020fac348528eda7396923199117c6e874a5c1` |
| `MainMenu_Standard_360x640.png` | `a507489ca9119217b7d008a7548bb56bd85694667ef7de2605b199d1abfef5f5` |
| `MainMenu_Standard_Grayscale.png` | `50c281d6fbd928a1725d0fc572d696259057fdca2305cdba9ca25cc2de9eba2e` |
| `MainMenu_TallSafe_1080x2160.png` | `732f460bd693e01975397d5aabeab155d377499b90913d28535a9985c8ae4a92` |
| `MainMenu_TabletSafe_1440x2560.png` | `cd0e4b515101d8e36a6de9b5ed71bc479f0ec9e7f970b6f21e223705c67a469d` |
| `MainMenu_Transitions.mov` | `026fbaf4389ab622a5257525949291719e998df64b07c481770ea7715fd2a19d` |
| `MainMenu_Transitions_ContactSheet.png` | `c67ce310ec8747c236ac4a0b00cb0ec586068e9f85bff69ecf25fb10ad42e19c` |
| `MainMenu_Transitions_Timing.csv` | `af68f38dc136301a475a3aff609f90078e730c352ee4a0d52629b5ec5d0a4e2` |

## Validation

- Focused MainMenu/controller/selector/glyph/presenter/token/integrity set: 41/41 in 2.51 s,
  Unity job `df57866a1563`.
- Complete EditMode suite: 173/173 in 21.40 s, Unity job `5d04452104b3`.
- Final compilation pipeline: zero errors and zero warnings.
- Scene and asset missing-reference scans: zero results.
- MainMenu saved, not dirty, with exactly one EventSystem.
- In-Play Console: zero errors and zero warnings.
- Controller interaction probe: Play, Stages, Store, Missions, and Remove Ads all locked together;
  Play, Stages, Store, Missions, and currently available Remove Ads restored together.
- Play-stop repeats the pre-existing `ModalInputBlockerPresenter.Dispose` destroyed-object
  exception at `Assets/Scripts/Game/UI/Services/ModalInputBlockerPresenter.cs:118`; no MainMenu
  production-pass, glyph, selector, transition, or validation-harness frame is in its stack.
- Illustrated-icon focused production/scene run: 168/168 in 21.19 s, Unity job `486a60f26cc4`.
- Complete current EditMode suite after replacing the procedural renderer: 168/168 in 21.20 s,
  Unity job `573bcc335a97`.
- Review-only Remove Ads / Currency revision: complete current EditMode suite 168/168 in 20.99 s,
  Unity job `ec33f4acf66e`; five approved runtime icon files remained byte-identical.
- Ring-free Remove Ads correction and defensive atlas packing: focused 5/5 in 1.05 s, Unity job
  `4742f1c895ce`; complete current EditMode suite 170/170 in 21.40 s, Unity job `c0eff8212144`;
  all six non-Remove-Ads runtime icons remained byte-identical.
- Default-knife / MainMenu-target production run: focused asset/scene coverage 7/7 in 1.34 s,
  Unity job `944772022707`; complete current EditMode suite 170/170 in 20.50 s, Unity job
  `f790a456c023`.
- Final Remove Ads proportion correction: focused MainMenu production coverage 7/7, Unity job
  `4e726650ceaa`; complete current EditMode suite 170/170, Unity job `9e4c1019f671`.
  All six non-Remove-Ads runtime icons remained byte-identical. The live 1080×1920 capture and its
  360×640 and grayscale derivatives were visually inspected with no hierarchy, alignment,
  legibility, silhouette, or proportion issues found.
- User-approved Remove Ads v5 integration: focused MainMenu production coverage 7/7 in 1.32 s,
  Unity job `4afc72bb192a`; complete EditMode suite 170/170 in 20.80 s, Unity job
  `c7c82b0a2822`. The live 1080×1920 capture and a nearest-neighbor close-up were visually
  inspected. The six non-Remove-Ads runtime icons remained byte-identical.

## Honest limits and remaining work

This is Unity Editor evidence only. It does not prove physical touch, player-build packing,
Android ASTC decode/memory, device safe insets, performance, or thermal behavior; P2-010/P8-006
remain the device matrix. A 4:3 landscape/wide tablet is not a supported layout for this locked
portrait project and was not claimed as proof; the documented tablet condition is portrait.

The newly authored default knife and MainMenu target now have demonstrated provenance. The 12
unlockable knife-skin sprites remain legacy content with unresolved source-right records and were
not changed. Generalized Store/Stages/Settings icon migration is still P6/P7 scope. UX-406 remains
unresolved: no shipping reduced-motion owner, preference, persistence, or default was added.
P3-004/P3-005 and the gameplay-HUD half of P3-008 remain open by scope.
