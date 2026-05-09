# Addressables: Android build, remote content, and new worlds

This guide covers building Addressable content for a **device APK**, configuring **remote** bundles (HTTPS), and adding a world under **`Assets/Configurations/Worlds`** so a built player can load it. For **Publishing order** (settings → build → upload → Load Path), see that section below.

It complements **`StageCollectionTheming-Setup.md`** (composite wiring and UI) and assumes worlds are loaded via **`StagesCollection`** Addressable entries referenced from **`StagesCollectionComposite`**.

---

## How this project loads worlds

- **`StagesCollectionComposite`** lists **`AssetReferenceT<StagesCollection>`** entries in order: world 0, 1, 2, …
- At runtime, **`WorldAssetService`** loads each world with **`Addressables.LoadAssetAsync<StagesCollection>`** using each reference’s runtime key.

### Why people say you “need a new APK” (current default wiring)

**`GameInstaller`** references **`StagesCollectionComposite`** as a normal asset on the prefab (direct **`ScriptableObject`** reference). Whatever is serialized in that asset—how many worlds, which **`AssetReference`** guids—**ships inside the APK** (or streaming assets / data with the build).

Remote Addressables only downloads assets **after** the game knows **what** to load (which addresses / keys). Right now that list lives **in the composite**. An old build literally has **no row** for “world 3” in memory, so uploading new bundles to the CDN does not add that row.

So:

- **Remote bundles** = how world *content* is stored and updated.
- **Composite (or another manifest)** = where the game learns *which worlds exist and in what order*. If that lives only in the APK, adding a new index requires a **new build**.

### What you actually want: no APK for a new world

You need the **world list** (and ideally **stage slot counts**) to come from **the same remote pipeline** as your collections:

1. **`StagesCollectionComposite` is itself an Addressable** (included in a **remote** group so its bundle can update), with **Build Remote Catalog** enabled on **`AddressableAssetSettings`**.
2. At startup (after **`CheckForCatalogUpdates` / `UpdateCatalogs`**), the game **loads the composite from Addressables** and uses **that** instance for **`CollectionsCount`**, **`GetCollectionLocationKey`**, **`GetStageSlotCountForWorld`**, etc.—instead of the prefab-serialized copy.
3. When you add **World 3** in the project: you extend the composite in the editor, rebuild Addressables, upload **catalog + bundles**. Devices on the **old** APK pull the **new catalog**, download the **new composite**, see a third entry, then download **`StagesCollection`** bundles as today.

Until the codebase is wired that way, the doc below still describes **content + installers** correctly, but **step “ship composite only remotely”** needs that bootstrap (see end of this file).

**Remote hosting** is for **AssetBundles / catalog** under **`Remote.LoadPath`**. It does not replace a baked composite until you **load the composite from there**.

---

## Prerequisites in Unity

1. **Addressables** package installed (this repo already references it).
2. **Android Build Support** installed via Unity Hub (Android SDK & NDK modules as needed for your Unity version).
3. A **host** for remote files: any HTTPS URL (S3 + CloudFront, Firebase Storage, GitHub Releases asset hosting, Azure Blob with public read, your own CDN, etc.) that returns **`catalog_*.json`**, **`*.hash`**, and **bundle** files with correct paths and **CORS** acceptable to **UnityWebRequest** (HTTPS strongly recommended; plain HTTP often fails on Android without extra manifest changes).

---

## Publishing order — settings → build → manual upload → Load Path (when you know it)

This matches **manual uploads**: Unity never pushes files for you. **`Remote.BuildPath`** is only **where bundles land on your disk**. **`Remote.LoadPath`** is **not** an upload step — it is the **HTTPS prefix baked into catalog/bundle metadata at build time**. Devices request URLs built from **`Remote.LoadPath` + relative paths**.

**You do not need the final CDN hostname on day one.** You **do** need a coherent plan:

| Step | What you do | Real public URL required? |
|------|-------------|-----------------------------|
| **1. Settings (Unity)** | **`Remote.BuildPath`** on disk, groups (composite + worlds), **`AddressableAssetSettings`** (**Part 1** tables). **`Remote.LoadPath`**: see below — avoid **`undefined`** when catalog uses **Remote**. | **No** for choosing **`Remote.BuildPath`**. For **`Remote.LoadPath`**, see **Deferring `Remote.LoadPath`**. |
| **2. Build files** | **New Build** → output under **`Remote.BuildPath`** (e.g. **`ServerData/Android/`**). | Uses whatever **`Remote.LoadPath`** was **at this build**. |
| **3. Upload manually** | Copy that folder tree with **FTP / S3 console / rsync / CI** — Unity is not involved. | Hosting gives you the URL prefix users hit. |
| **4. Fix Load Path + rebuild if needed** | Paste your real HTTPS prefix into **`Remote.LoadPath`** (**Manage Profiles**). If that string was wrong or missing when you ran **New Build**, run **New Build** again and upload again — old catalogs keep old URLs. | **Yes** before shipping an APK that downloads Addressables from the web. |

### Deferring `Remote.LoadPath` (you upload manually first)

- **`undefined` Load Path preview** means the profile variable **`Remote.LoadPath`** is empty — Unity often cannot treat **Remote** catalog/pathing as sane; fix before relying on remote downloads.
- **If you truly don’t know the final URL yet**, pick **one** of these:
  - **Know the path shape early**: set **`Remote.LoadPath`** to the **final prefix pattern** as soon as you know only the **folder path** (example: `https://YOUR_REAL_DOMAIN/game/Android/`). DNS can come later; **rebuild only if the string changes**.
  - **Unknown prefix until after upload**: use a **temporary** HTTPS URL whose **path suffix** matches where you **intend** files to live (`…/Android/`). Finish step **2–3**. When hosting gives the **real** prefix, put that exact string into **`Remote.LoadPath`**, run **New Build**, **manual upload again**. Accept **two uploads** — first batch was only for staging unless you guessed right first try.

There is **no** Unity setting that means “figure out Load Path from manual upload automatically.” **Manual upload always comes from disk output; Load Path always comes from you typing the matching public URL into the profile**, then **build → upload** align.

---

## Part 1 — Addressables profile: local vs remote

1. Open **Window → Asset Management → Addressables → Groups**.
2. Open **Manage Profiles** (toolbar on the Groups window; exact label may be **Profile** dropdown → **Manage**).
3. Use the active profile (often **Default**) and set at least:
   - **Local.BuildPath** / **Local.LoadPath** — defaults are fine for content embedded or copied with the player (see Unity docs for your version).
   - **Remote.BuildPath** — folder on disk where Unity writes remote artifacts when you build (example pattern often looks like `ServerData/[BuildTarget]`). **Set this first** — no CDN needed.
   - **Remote.LoadPath** — HTTPS prefix devices use **after** upload (**Publishing order** above). Defer hostname using **Deferring `Remote.LoadPath`**, not by leaving it empty indefinitely.

4. Select **`AddressableAssetSettings`** in the Project window (`Assets/AddressableAssetsData/AddressableAssetSettings.asset`). Use the picks below in the Inspector (**exact labels match Unity’s Addressables package UI**; minor wording can vary by version).

### Addressable Asset Settings inspector — what to pick

Open **`Assets/AddressableAssetsData/AddressableAssetSettings.asset`**. Work through **Catalog** first; **`Build & Load Paths`** here reads from the **same profile variables** you edit under **Manage Profiles** (**Remote.BuildPath**, **Remote.LoadPath**).

**Catalog**

| Inspector control | What to choose | Why |
|-------------------|----------------|-----|
| **Player Version Override** | **`[UnityEditor.PlayerSettings.bundleVersion]`** (default) | Keeps catalog/version alignment with **Player Settings → Version**. Change only if your CI uses a different versioning variable on purpose. |
| **Build Remote Catalog** | **Enabled (checked)** | Lets players download a **new catalog** from your CDN after launch (required for “update worlds without APK” together with code that calls **`CheckForCatalogUpdates`** / **`UpdateCatalogs`**). |
| **Build & Load Paths** | **Remote** | Catalog files are produced under **`Remote.BuildPath`** and are expected to be downloaded from **`Remote.LoadPath`** at runtime. Pick **Local** only if you intentionally ship catalogs only inside the player and never update them remotely. |
| **Path preview → Build Path** | Should show a resolved disk folder (e.g. **`ServerData/Android`**) | Comes from **`Remote.BuildPath`** + active **Build Target**. Switch **File → Build Settings → Android** before judging paths. |
| **Path preview → Load Path** | Your **HTTPS base URL** for uploaded files — not **`<undefined>`** when using **Remote** catalog | **`<undefined>`** = empty **`Remote.LoadPath`** in **Manage Profiles**. Fill it **before** builds that players will download — use the **final** prefix or a **temporary** HTTPS URL you will replace (**Publishing order → Deferring `Remote.LoadPath`**). Trailing **`/`** must match how your CDN paths combine with catalog entries. |
| **Enable Json Catalog** | **Unchecked** for normal ships | Optional human-readable / tooling choice; leave off unless you explicitly need JSON catalogs. |
| **Only update catalogs manually** | **Unchecked** | Leave off so runtime catalog checks (as in this project’s **`CatalogUpdateService`**) can fetch updates. Turn on only if you deliberately disable automatic catalog downloads. |
| **Internal Asset Naming Mode** | **`Full Path`** (or project default) | Fine for this pipeline unless Unity release notes for your version recommend otherwise. |
| **Internal Bundle Id Mode** | **`Group Guid Project Id Hash`** (typical default) | Stable bundle naming; keep default unless your build scripts require another mode. |
| **Asset Load Mode** | **`Requested Asset And Dependencies`** | Normal dependency loading for referenced assets. |
| **Asset Provider** / **Asset Bundle Provider** | Defaults (**Assets from Bundles Provider**, **Asset Bundle Provider**) | Change only when using custom providers. |

**Update a previous build**

| Inspector control | What to choose |
|-------------------|----------------|
| **Check for Update Issues** | **`List Restricted Assets (recommended)`** — acceptable default. |
| **Content State Build Path** | **`<default settings path>`** unless your team uses a custom content-state workflow. |

**Downloads**

| Inspector control | What to choose |
|-------------------|----------------|
| **Custom certificate handler** | **`<none>`** unless you terminate TLS with a cert Unity cannot validate and you install a custom handler. |
| **Max Concurrent Web Requests** | **`3`** (default) is fine; raise slightly only if profiling shows download stalls and your CDN allows it. |
| **Catalog download timeout** / **Bundle request timeout** | **`0`** = default engine behavior; set explicit seconds only when debugging slow networks. |
| **Bundle retry count** | **`0`** unless you want automatic retries on flaky networks (trade-offs with duplicate failures in logs). |
| **Use UnityWebRequest for Local Assets** | **Unchecked** unless Unity docs for your case tell you otherwise. |

**Also on `AddressableAssetSettings` (outside these sections)**

- **Build Addressables on Player Build**: optional; many teams leave it off and run **Build → New Build** manually (**Part 3**).

**Play Mode (Editor):** In the Groups window, **Play Mode Script → Use Asset Database** for fast iteration without building bundles. For testing **Use Existing Build**, run a full **New Build** first.

---

## Part 2 — Remote group for world collections

For worlds you want downloaded from the CDN (not packed only inside StreamingAssets):

1. **Window → Asset Management → Addressables → Groups → Create → Packed Assets** (or duplicate an existing packed group).
2. Name it e.g. **`Remote_Worlds`**.
3. Open the group’s **Bundled Asset Group** schema (Inspector):
   - Set **Pathing** to the **Remote** path pair (**Remote.BuildPath** / **Remote.LoadPath**), not Local—so built bundles and the runtime internal IDs point at your URL.
4. Drag each world’s **`StagesCollection`** asset into this group (or mark them addressable and assign the group). Give each collection a **unique Address** (same Inspector field as the composite — **Addressables vocabulary**) consistent with how you reference it from the composite (**AssetReference** stores GUIDs; the Address label is mainly for you and Addressables tooling).

**Dependency assets** (stages, textures, prefabs referenced by that `StagesCollection`) are usually **included as dependencies** of the collection’s bundle when you build; you do not have to mark every file Addressable unless you want separate addresses or bundles.

**Note:** This repo also has an auto-generated **URP**-related group (e.g. **unifiedraytracing**); leave engine-owned groups as-is unless you know you need to change them.

---

## Addressables vocabulary — Primary Key, Address, GUID (composite)

Unity overloads similar words; this is what each means **in this project** and **where you edit it**.

| Term | Meaning | Where you see / edit it |
|------|---------|-------------------------|
| **Addressable entry** | This asset is registered with Addressables: it appears in a **group**, gets bundled/catalogued on **New Build**, and can be loaded at runtime. | **Window → Asset Management → Addressables → Groups** — your asset appears as a **row** under some group. |
| **Group** | A pack of entries sharing **Build Path / Load Path** (Local vs Remote). | Same **Groups** window — left column list (e.g. **`Remote_Config`**, **`Remote_Worlds`**). Click group → **Inspector** sets **Bundled Asset Group** pathing. |
| **Primary Key** | Unity’s catalog name for the **default address string** for that entry. In the Editor this field is almost always labeled **Address** (not “Primary Key”) on the **selected entry’s Inspector**. | **Groups** window → **click the composite row** (the line for **`StagesCollectionComposite`**) → **Inspector** (often docked right): field **Address**. Alternative: select **`StagesCollectionComposite.asset`** in **Project** — some setups show an **Addressables** strip/foldout with **Address** when the asset is already addressable. |
| **GUID** | Stable ID of the asset file on disk (what **`AssetReference`** serializes). | Not typed by hand for normal workflow — Unity fills it when you assign the asset on **`GameInstaller` → Remote Stages Collection Composite Reference**. |

**What actually loads the composite in this codebase**

- **`RemoteCompositeLoader`** calls **`LoadAssetAsync`** with **`AssetReference.RuntimeKey`**.
- For a normal **`AssetReference`** (no sub-asset), **`RuntimeKey`** is the asset **GUID string**, **not** the **Address** text.
- Therefore **`Address`** does **not** have to be `Config/StagesCollectionComposite` for Knife Toss to run. It must only be **unique** in your catalog if you set it; duplicate addresses break builds.

**Why the docs ever suggested `Config/StagesCollectionComposite`**

- **Human readability** in **Groups** and in catalog files when debugging (“which row is the composite?”).
- Optional future use: code or tools that load by **`LoadAssetAsync("Config/StagesCollectionComposite")`** — **this repo does not do that** for the composite today.

**What you must do vs optional**

- **Must:** Mark **`StagesCollectionComposite.asset`** addressable, put it in a **Remote**-pathed group, assign **`GameInstaller` → Remote Stages Collection Composite Reference** to **that same asset**, build + upload (**Publishing order**).
- **Optional:** Set **Address** to something stable and readable (example: **`Config/StagesCollectionComposite`**) or leave Unity’s auto-generated address — runtime still uses **GUID** via **`AssetReference`** as wired.

---

## Part 3 — Building Addressables, manual upload, then Load Path alignment

Follow **Publishing order** at the top of this file: **disk settings + optional provisional Load Path → build → manual upload → finalize Load Path + rebuild if URLs changed**.

1. Switch platform: **File → Build Settings → Android → Switch Platform** (recommended before building Addressables so `[BuildTarget]` resolves to Android).
2. **Window → Asset Management → Addressables → Build → New Build → Default Build Script** (or your team’s chosen script).
3. After the build:
   - **Local** output goes under the configured **Local.BuildPath** (often under the project / `Library` / temp locations depending on profile—check the Addressables build log window).
   - **Remote** output goes under **Remote.BuildPath** (e.g. `ServerData/Android/`): bundles, **catalog**, and **hash** files.

4. **Manual upload:** Copy **everything** under that remote output folder using **your** tooling (no Unity upload). Preserve paths.

5. **`Remote.LoadPath`:** Paste the same HTTPS prefix your hosting gives you (e.g. Unity Cloud bucket → **Addressable Remote Path URLs** → **Badge: latest**). It must match where step **4** uploaded files. If this string changed since step **2**, run **New Build** (step **2**) and upload again (step **4**).

6. **Verify:** In a browser, open one catalog URL and one bundle URL built from that prefix — **200** OK, **404** means wrong path or stale catalog from step **2**.

---

## Part 4 — Building an APK for testing

1. **File → Build Settings**
   - Select **Android**.
   - Add the **bootstrap** scenes your game needs (at least the first scene that creates **`GameInstaller`** / loads the game).
2. **Player Settings** (gear icon or **Edit → Project Settings → Player**)
   - **Company / Product** names as needed.
   - **Android** tab:
     - **Minimum API Level** compatible with your dependencies (Unity and vendors document requirements).
     - **Scripting Backend**: **IL2CPP** is typical for store builds; **Mono** can speed some internal tests.
     - **Target API Level** per store policy (Google Play raises this over time).
     - **Internet Access**: **Require** if you rely on remote Addressables (often already required by ads/analytics).
3. **Keystore** (optional for local sideload): use **Project Keystore** or a debug workflow your team uses.
4. **Build** or **Build And Run** to produce **`.apk`** (or **App Bundle** `.aab` if that is your pipeline).

**First-run test:** Install on device after **`Remote.LoadPath`** matches the uploaded catalog (**Publishing order**, step **4**). Ensure Wi‑Fi/data allows HTTPS to that host, then open the world menu and load a remote-backed world. Use **adb logcat** filtered by **Unity** / **Addressables** if loads fail.

---

## Part 5 — Adding a new world (example: `World3`) and publishing it

### A. In the Unity project (content)

1. Under **`Assets/Configurations/Worlds`**, use or create a folder such as **`World3`**.
2. Add **`StageConfig`** assets, art, **`StagesCollection_3`** (or your naming), particle prefab, etc., same pattern as **`World1` / `World2`**.
3. Mark **`StagesCollection_3`** (or whichever asset represents the world) as **Addressable** and place it in **`Remote_Worlds`** (if that world should download from CDN).

### B. Composite and progression

1. Open **`StagesCollectionComposite.asset`** (current path in repo: **`Assets/Configurations/Worlds/StagesCollectionComposite.asset`**).
2. Add an entry to **Collection References**: assign the **same** `StagesCollection` object you marked Addressable (drag from Project so GUID/runtime key stay aligned).
3. Set **Stage Slot Counts Per World** for that index to the number of stages in that world (must match the list length used for unlock/UI math).

### C. Installers and menus

- **`GameInstaller`** and **`StageInstaller`** must still reference this **same** composite asset.
- No code change is required for a new world if it is just another slot in the composite and uses the same `StagesCollection` pipeline.

### D. Build, manual upload, Load Path, verify

1. Confirm **Part 1** (**Remote.BuildPath**, groups) and **Part 2** before building. **`Remote.LoadPath`** either final or explicitly provisional (**Publishing order**).
2. **Build Addressables** (**Part 3**, steps 1–3) for **Android**.
3. **Manual upload** (**Part 3**, step 4).
4. Align **`Remote.LoadPath`** with real hosting (**Part 3**, step 5); **rebuild + re-upload** if the baked prefix was wrong.
5. **Verify** (**Part 3**, step 6).

**APK vs no APK:**

| Composite source | Add World 3 to composite in editor, publish Addressables | New APK? |
|------------------|-----------------------------------------------------------|----------|
| On **`GameInstaller`** only (baked reference) | Same as today: device still uses old serialized list | **Yes** — must ship updated composite inside the build |
| Loaded from **Addressables** at startup (fixed address) + **remote catalog** update | Upload new composite + world bundles + catalog | **No** — devices pick up the new list after catalog update |

After wiring the **remote composite** path, the app resolves the new world’s **AssetReference** from the downloaded composite; bundle bytes still load from **Remote.LoadPath** as today.

---

## Troubleshooting (short)

| Symptom | Things to check |
|--------|------------------|
| 404 / failed download | **Remote.LoadPath** trailing slash, folder layout vs catalog **Internal Id**, HTTPS reachability from device. |
| Old world shows, new does not | Baked composite not updated in build **or** remote composite / catalog not loaded; or collection not Addressable / wrong group. |
| Works in Editor, fails on device | Editor **Use Asset Database** vs device using real bundles; run **Use Existing Build** in Editor to reproduce. |
| SSL / certificate errors | Device time wrong, self-signed cert, or outdated TLS on server. |

---

## Reference — important assets/paths in this repo

| Item | Path |
|------|------|
| Composite | `Assets/Configurations/Worlds/StagesCollectionComposite.asset` |
| Addressables settings | `Assets/AddressableAssetsData/AddressableAssetSettings.asset` |
| World content (example) | `Assets/Configurations/Worlds/World1`, `World2`, `World3`, … |

---

## Code / DI changes for “no APK” (not optional for that goal)

The game must **not** treat the prefab-assigned composite as the only source of truth. A minimal direction:

1. Set **Remote.BuildPath**, configure **`Remote.LoadPath`** per **Publishing order** (final or provisional), and enable **Build Remote Catalog** in **`AddressableAssetSettings`**.
2. Mark **`StagesCollectionComposite.asset`** as **Addressable** (unique **Address** label optional but practical for humans; runtime loading here uses **GUID** via **`AssetReference`** on **`GameInstaller`**).
3. Put it in a **Remote** group (or local for very first ship, then remote for updates—your pipeline choice).
4. During startup: **`Addressables.InitializeAsync`** (if needed) → **`CheckForCatalogUpdates`** / **`UpdateCatalogs`** → **`LoadAssetAsync<StagesCollectionComposite>(address)`** → use that instance for **`WorldAssetService`**, **`StageListController`**, **`PlayerDataService`** clamp/unlock helpers, **`GameStateMachine`**, etc.
5. Keep a **fallback** (Editor or offline stub) if load fails (optional but recommended).

Today **`GameInstaller`** registers the prefab’s **`StagesCollectionComposite`** instance with VContainer. That registration must switch to the **runtime-loaded** instance once load completes, or use a small holder service **`IStagesCollectionCatalog`** that exposes the composite after async init.

Also add **`Addressables.CheckForCatalogUpdates`** / **`UpdateCatalogs`** (and log via **`ILogger`**) so a stored APK actually pulls a new **catalog** before loading the composite.

---

## Implemented runtime wiring (this repo)

- **`CatalogUpdateService`** (`Common.AssetLoading.UnityAddressables`): wraps **`CheckForCatalogUpdates`** / **`UpdateCatalogs`** with graceful failures when offline or unchanged.
- **`RemoteCompositeLoader`** (`Game.Level.Services`): loads **`StagesCollectionComposite`** from Addressables using the **`AssetReferenceStagesCollectionComposite`** assigned on **`GameInstaller`** (resolved via **`RuntimeKey`** / GUID). On failure, downstream code keeps using the fallback below.
- **`StagesCollectionCatalogProvider`** implements **`IStagesCollectionCatalog`**: returns the remote-loaded composite when present; otherwise the **`StagesCollectionComposite`** assigned on **`GameInstaller`** (serialized fallback).
- **`LoadingController`** (Loading scene entry point): **`CatalogUpdateService.UpdateCatalogAsync`**, then **`RemoteCompositeLoader.LoadAsync`**, then **`SceneLoaderService.LoadMainMenu`**. **`WorldAssetService`**, **`StageListController`**, **`MainMenuController`**, **`GameStateMachine`**, and **`PlayerDataService`** resolve **`IStagesCollectionCatalog`** instead of the composite type directly.

### Unity editor checklist — remote world list (`StagesCollectionComposite`)

**What this does:** The game can download an updated **world list** (the composite asset) from your server after launch, so you do not need a new APK every time you add a world. **`GameInstaller`** must assign **`AssetReferenceStagesCollectionComposite`** to the **same** **`StagesCollectionComposite`** asset as the serialized fallback — that assignment supplies the **GUID** **`RemoteCompositeLoader`** uses via **`RuntimeKey`**. You still mark the composite **Addressable** in a remote-backed group so it exists in the catalog and bundle layout (**Addressables vocabulary** explains **Address** vs GUID). If the asset is not addressable or hosting/catalog URLs are wrong, remote load fails and the game falls back to the direct **`StagesCollectionComposite`** on **`GameInstaller`**.

Checklist order matches **Publishing order**: **Unity disk/settings → build → manual upload → set/fix Load Path → verify** (second build + upload when Load Path was unknown at first build).

#### 1. Settings on disk and in Unity (CDN hostname optional)

1. Open **Window → Asset Management → Addressables → Groups**.
2. **Manage Profiles** (toolbar). Pick the active profile (often **Default**).
3. Set **`Remote.BuildPath`** (where **`ServerData/Android/`**-style output appears). **No URL needed.**
4. Set **`Remote.LoadPath`** using **Publishing order → Deferring `Remote.LoadPath`**: either your **final** HTTPS prefix, or a **provisional** prefix you accept replacing later (**New Build** + upload again). Avoid leaving it empty (**`<undefined>`** preview breaks **Remote** sanity).
5. Open **`Assets/AddressableAssetsData/AddressableAssetSettings.asset`**. Enable **Build Remote Catalog**, set **Build & Load Paths** to **Remote**, confirm **Path preview** matches expectations. Full control-by-control picks: **Part 1 → Addressable Asset Settings inspector — what to pick**.
6. Put **`StagesCollectionComposite`** in Addressables (**terms**: **Addressables vocabulary**):
   - In **Project**, select **`Assets/Configurations/Worlds/StagesCollectionComposite.asset`** (repo path; yours may differ).
   - **Window → Asset Management → Addressables → Groups**.
   - Drag that asset into a group (or **Create → Packed Assets**, e.g. **`Remote_Config`**). Confirm the asset appears as an **entry row** under that group.
   - Select the **group** → **Inspector** → **Bundled Asset Group** schema → **Build Path** / **Load Path** (or **Pathing**) = **Remote** (**`Remote.BuildPath`** / **`Remote.LoadPath`**), not Local-only for bundles you intend to host remotely.
   - Select the **composite entry row** (single click the row for **`StagesCollectionComposite`**, not only the group) → **Inspector**: **Address** field — optional readable label (example: **`Config/StagesCollectionComposite`**) **or** leave Unity’s default, as long as the value stays **unique** in the catalog. Runtime load for this game uses the asset **GUID** from **`GameInstaller`**, not this **Address** string — see **Addressables vocabulary**.
7. Open **`Assets/Prefabs/GameInstaller.prefab`** → **`GameInstaller`** component → **`Remote Stages Collection Composite Reference`**: assign **the same** **`StagesCollectionComposite`** asset. That wires **`AssetReference`** → GUID → runtime load.

Repeat steps **6–7** when you add worlds (**Part 2**, **Part 5**) so collections and the composite stay in remote-backed groups with consistent pathing.

#### 2. Build files (local output)

8. **File → Build Settings → Android → Switch Platform** if you ship Android (so **`[BuildTarget]`** resolves correctly).
9. **Window → Asset Management → Addressables → Build → New Build → Default Build Script** (or your team script).
10. Confirm output under **`Remote.BuildPath`** (often **`ServerData/Android/`**): **bundles**, **`catalog_*.json`**, and **hash** files.

#### 3. Upload manually

11. Upload **everything** from that folder **yourself** (FTP, S3, host panel, CI artifact promotion — not Unity). Preserve folder layout.

#### 4. After upload

12. Unity Cloud → **Content Delivery** → your bucket → **Addressable Remote Path URLs** → copy **Badge: latest**. Unity Editor → **Addressables → Groups → Manage Profiles → Default** → **`Remote.LoadPath`** → paste that URL (add trailing **`/`** if your preview expects it).

13. If **`Remote.LoadPath`** was wrong or empty **when you ran step 9**: run **step 9** again, then **step 11** again (catalog remembers the old prefix otherwise).

14. Run the game on device with internet. Still broken → **`adb logcat`** (**Unity** / **Addressables**). **`RemoteCompositeLoader`** failing → app uses **`GameInstaller`** fallback composite until URL + build + upload match.

If step **13** applied and it still fails, **`Remote.LoadPath`** likely doesn’t match the URL hosting actually serves (slashes and path matter).
