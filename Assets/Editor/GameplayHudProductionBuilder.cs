#if UNITY_EDITOR
using System;
using System.Linq;
using Game.PowerUps.Views;
using Game.UI.HUD;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayHudProductionBuilder
{
    private const string HudArt = "Assets/Art/UI/HUD/";
    private const string StagePrefabPath = "Assets/Prefabs/UI/HUD/StageProgressPlaque.prefab";
    private const string MagazinePrefabPath = "Assets/Prefabs/UI/HUD/KnifeMagazineDock.prefab";
    private const string ProgressSlotPath = "Assets/Prefabs/UI/ProgressBarSlot.prefab";
    private const string KnifeIconPath = "Assets/Prefabs/MagazineKnifeIcon.prefab";
    private const string PowerUpButtonPath = "Assets/Prefabs/UI/PowerUp/PowerUpButton.prefab";
    private const string SettingsButtonPath = "Assets/Prefabs/UI/Buttons/MenuButton.prefab";
    private const string CurrencyDisplayPath = "Assets/Prefabs/UI/Coin/CoinUiDisaply.prefab";
    private const string BoldFontPath = "Assets/Art/UI/Fonts/BarlowCondensed-ExtraBold SDF.asset";
    private const string DefaultFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Tools/Toss Madness/UI/Build Approved Gameplay HUD")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Build the Gameplay HUD outside Play Mode.");

        EnsureFolder("Assets/Prefabs/UI/HUD");
        ConfigureHudTextures();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Sprite stageShell = LoadSprite("HudStageProgressShell.png");
        Sprite magazineShell = LoadSprite("HudKnifeMagazineShell.png");
        Sprite progressMarker = LoadSprite("HudProgressMarker.png");

        UpdateProgressSlot(progressMarker);
        UpdateKnifeIcon();
        UpdateSettingsButton();
        UpdateStagePlaque(stageShell);
        UpdateKnifeMagazine(magazineShell);
        UpdateGameplayCurrency();
        UpdateSceneLayout();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(ValidateInternal());
    }

    [MenuItem("Tools/Toss Madness/UI/Validate Approved Gameplay HUD")]
    public static void Validate()
    {
        Debug.Log(ValidateInternal());
    }

    private static void ConfigureHudTextures()
    {
        ConfigureTexture("HudStageProgressShell.png", new Vector4(130, 86, 130, 86), 1024);
        ConfigureTexture("HudKnifeMagazineShell.png", new Vector4(86, 130, 86, 130), 1024);
        ConfigureTexture("HudPowerUpBezel.png", Vector4.zero, 512);
        ConfigureTexture("HudCountBadge.png", Vector4.zero, 256);
        ConfigureTexture("HudProgressMarker.png", Vector4.zero, 128);
    }

    private static void ConfigureTexture(string fileName, Vector4 border, int maxSize)
    {
        string path = HudArt + fileName;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Missing HUD texture importer: {path}");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = maxSize;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
    }

    private static void UpdateProgressSlot(Sprite marker)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ProgressSlotPath);
        try
        {
            root.layer = 5;
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(22, 22);
            rect.localRotation = Quaternion.identity;

            var image = root.GetComponent<Image>();
            image.sprite = marker;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            var layout = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            layout.preferredWidth = 22;
            layout.preferredHeight = 22;
            layout.flexibleWidth = 0;
            layout.flexibleHeight = 0;

            var fitter = root.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                UnityEngine.Object.DestroyImmediate(fitter);

            PrefabUtility.SaveAsPrefabAsset(root, ProgressSlotPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void UpdateKnifeIcon()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(KnifeIconPath);
        try
        {
            root.layer = 5;
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(52, 76);
            rect.localRotation = Quaternion.Euler(0, 0, 27.79f);

            var image = root.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;

            var layout = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            layout.preferredWidth = 52;
            layout.preferredHeight = 76;
            layout.flexibleWidth = 0;
            layout.flexibleHeight = 0;

            PrefabUtility.SaveAsPrefabAsset(root, KnifeIconPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void RestorePowerUpButton()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PowerUpButtonPath);
        try
        {
            root.layer = 5;
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 200);

            var rootImage = root.GetComponent<Image>();
            rootImage.sprite = null;
            rootImage.type = Image.Type.Simple;
            rootImage.preserveAspect = false;
            rootImage.color = Color.clear;
            rootImage.raycastTarget = true;

            var button = root.GetComponent<Button>();
            button.transition = Selectable.Transition.None;

            Transform iconTransform = root.transform.Find("Icon");
            var iconRect = (RectTransform)iconTransform;
            SetStretch(iconRect, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f));
            var icon = iconTransform.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            button.targetGraphic = icon;

            Transform countTransform = root.transform.Find("Count");
            var countRect = (RectTransform)countTransform;
            SetCorner(countRect, new Vector2(0.5f, 1), new Vector2(0, 8), new Vector2(78, 64));
            var countImage = countTransform.GetComponent<Image>();
            countImage.sprite = LoadSpriteByName("Assets/Art/PowerUps/PowerUpBadges.png", "CountChip");
            countImage.color = Color.white;
            countImage.preserveAspect = true;
            countImage.raycastTarget = false;

            TMP_Text countText = countTransform.GetComponentInChildren<TMP_Text>(true);
            countText.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontPath);
            countText.fontSize = 44;
            countText.fontStyle = FontStyles.Bold;
            countText.alignment = TextAlignmentOptions.Center;
            countText.color = new Color(0.176f, 0.106f, 0.071f, 1f);
            countText.raycastTarget = false;
            countText.enableAutoSizing = true;
            countText.fontSizeMin = 26;
            countText.fontSizeMax = 44;
            var countShadow = countText.GetComponent<Shadow>();
            if (countShadow != null)
                UnityEngine.Object.DestroyImmediate(countShadow);

            Transform plusTransform = root.transform.Find("PlusButton");
            var plusRect = (RectTransform)plusTransform;
            SetCorner(plusRect, new Vector2(0.5f, 1), new Vector2(0, 8), new Vector2(66, 66));
            var plusImage = plusTransform.GetComponent<Image>();
            plusImage.sprite = LoadSpriteByName("Assets/Art/PowerUps/PowerUpBadges.png", "AcquisitionPlus");
            plusImage.preserveAspect = true;
            plusImage.raycastTarget = false;
            Transform glyphTransform = plusTransform.Find("Glyph");
            if (glyphTransform != null)
                UnityEngine.Object.DestroyImmediate(glyphTransform.gameObject);

            var view = root.GetComponent<PowerUpButtonView>();
            SetObjectReference(view, "_Button", button);
            SetObjectReference(view, "_Icon", icon);
            SetObjectReference(view, "_CountText", countText);
            SetObjectReference(view, "_CountRoot", countTransform.gameObject);
            SetObjectReference(view, "_PlusRoot", plusTransform.gameObject);
            SetColor(view, "_SelectedColor", Color.white);
            SetColor(view, "_UnselectedColor", Color.white);
            SetColor(view, "_UnavailableColor", new Color(0.52f, 0.52f, 0.52f, 1f));

            SetLayerRecursively(root.transform, 5);
            PrefabUtility.SaveAsPrefabAsset(root, PowerUpButtonPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void UpdateSettingsButton()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(SettingsButtonPath);
        try
        {
            root.layer = 5;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(112, 112);

            Transform holder = root.transform.Find("Holder");
            SetStretch((RectTransform)holder, Vector2.zero, Vector2.one);

            Transform buttonTransform = holder.Find("Button");
            SetStretch((RectTransform)buttonTransform, Vector2.zero, Vector2.one);
            var surface = buttonTransform.GetComponent<Image>();
            surface.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Core/ButtonIconSocket.png");
            surface.type = Image.Type.Sliced;
            surface.preserveAspect = false;
            surface.color = Color.white;
            surface.raycastTarget = true;

            var button = buttonTransform.GetComponent<Button>();
            button.targetGraphic = surface;

            Transform iconTransform = buttonTransform.childCount > 0
                ? buttonTransform.GetChild(0)
                : CreateUiObject("Icon", buttonTransform).transform;
            iconTransform.name = "Icon";
            SetStretch((RectTransform)iconTransform, new Vector2(0.19f, 0.19f), new Vector2(0.81f, 0.81f));
            var icon = iconTransform.GetComponent<Image>() ?? iconTransform.gameObject.AddComponent<Image>();
            icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icons/IconSettings.png");
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            WireAnimatorGraphic(root, surface);
            SetLayerRecursively(root.transform, 5);
            PrefabUtility.SaveAsPrefabAsset(root, SettingsButtonPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void UpdateStagePlaque(Sprite shell)
    {
        GameObject root = FindSceneObject("StageProgressBar");
        if (root == null)
            throw new InvalidOperationException("StageProgressBar was not found in StagePlay.");

        UnpackRootIfNeeded(root);
        bool sceneActive = root.activeSelf;
        root.SetActive(true);
        ClearChildren(root.transform);
        root.layer = 5;

        var rect = root.GetComponent<RectTransform>();
        SetCorner(rect, new Vector2(0.5f, 1f), new Vector2(0, -82), new Vector2(430, 155));

        var shellImage = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        shellImage.enabled = true;
        shellImage.sprite = shell;
        shellImage.type = Image.Type.Simple;
        shellImage.color = Color.white;
        shellImage.raycastTarget = false;

        TMP_Text stageLabel = CreateText("StageLabel", root.transform, "STAGE 1", 43,
            new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.94f), Color.white);
        stageLabel.characterSpacing = 1f;

        GameObject slotsObject = CreateUiObject("ProgressMarkers", root.transform);
        var slotsRect = slotsObject.GetComponent<RectTransform>();
        SetStretch(slotsRect, new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.57f));
        var layout = slotsObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var view = root.GetComponent<StageProgressBarView>() ?? root.AddComponent<StageProgressBarView>();
        SetObjectReference(view, "_Container", slotsObject.transform);
        SetObjectReference(view, "_SlotPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ProgressSlotPath));
        SetColor(view, "_CompletedColor", new Color(1f, 0.77f, 0.12f, 1f));
        SetColor(view, "_CurrentLevelColor", new Color(1f, 0.95f, 0.55f, 1f));
        SetColor(view, "_IncompleteColor", new Color(0.25f, 0.21f, 0.17f, 1f));
        SetObjectReference(view, "_CurrentStageLabel", stageLabel);
        SetObjectReference(view, "_NextStageLabel", null);

        WireAnimatorGraphic(root, shellImage);
        SetLayerRecursively(root.transform, 5);
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, StagePrefabPath, InteractionMode.AutomatedAction);
        root.SetActive(sceneActive);
    }

    private static void UpdateGameplayCurrency()
    {
        GameObject current = FindSceneObject("CoinUiDisaply");
        if (current == null)
            throw new InvalidOperationException("CoinUiDisaply was not found in StagePlay.");

        Transform parent = current.transform.parent;
        int siblingIndex = current.transform.GetSiblingIndex();
        bool sceneActive = current.activeSelf;
        var sharedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CurrencyDisplayPath);
        if (sharedPrefab == null)
            throw new InvalidOperationException($"Missing shared currency prefab: {CurrencyDisplayPath}");

        UnityEngine.Object.DestroyImmediate(current);
        GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(
            sharedPrefab,
            EditorSceneManager.GetActiveScene());
        root.name = "CoinUiDisaply";
        root.transform.SetParent(parent, false);
        root.transform.SetSiblingIndex(siblingIndex);
        root.SetActive(sceneActive);

        var rootRect = root.GetComponent<RectTransform>();
        SetCorner(rootRect, new Vector2(1, 1), new Vector2(-164, -76), new Vector2(384, 112));
        rootRect.localScale = new Vector3(0.78f, 0.78f, 1f);
        PrefabUtility.RecordPrefabInstancePropertyModifications(rootRect);
    }

    private static void UpdateKnifeMagazine(Sprite shell)
    {
        GameObject root = FindSceneObject("KnifeMagazine");
        if (root == null)
            throw new InvalidOperationException("KnifeMagazine was not found in StagePlay.");

        UnpackRootIfNeeded(root);
        bool sceneActive = root.activeSelf;
        root.SetActive(true);
        ClearChildren(root.transform);
        root.layer = 5;

        var rect = root.GetComponent<RectTransform>();
        SetCorner(rect, new Vector2(0, 0), new Vector2(100, 32), new Vector2(164, 520), new Vector2(0.5f, 0));

        var shellImage = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        shellImage.enabled = true;
        shellImage.sprite = shell;
        shellImage.type = Image.Type.Simple;
        shellImage.color = Color.white;
        shellImage.raycastTarget = false;

        GameObject iconsObject = CreateUiObject("KnifeIcons", root.transform);
        var iconsRect = iconsObject.GetComponent<RectTransform>();
        SetStretch(iconsRect, new Vector2(0.14f, 0.09f), new Vector2(0.86f, 0.91f));
        var layout = iconsObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var oldLayout = root.GetComponent<VerticalLayoutGroup>();
        if (oldLayout != null)
            UnityEngine.Object.DestroyImmediate(oldLayout);

        var display = root.GetComponent<KnifeMagazineDisplay>() ?? root.AddComponent<KnifeMagazineDisplay>();
        SetObjectReference(display, "_Container", iconsObject.transform);
        SetObjectReference(display, "_IconPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(KnifeIconPath));
        SetColor(display, "_ActiveColor", Color.white);
        SetColor(display, "_InactiveColor", new Color(0.34f, 0.3f, 0.24f, 1f));

        WireAnimatorGraphic(root, shellImage);
        SetLayerRecursively(root.transform, 5);
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, MagazinePrefabPath, InteractionMode.AutomatedAction);
        root.SetActive(sceneActive);
    }

    private static void UpdateSceneLayout()
    {
        GameObject menu = FindSceneObject("MenuButton");
        if (menu != null)
        {
            SetCorner(menu.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(68, -76), new Vector2(112, 112));
            PrefabUtility.RecordPrefabInstancePropertyModifications(menu.GetComponent<RectTransform>());
        }

        GameObject powerUps = FindSceneObject("PowerUps");
        if (powerUps != null)
        {
            var powerUpsRect = powerUps.GetComponent<RectTransform>();
            powerUpsRect.anchorMin = new Vector2(1, 0);
            powerUpsRect.anchorMax = new Vector2(1, 0.5f);
            powerUpsRect.pivot = new Vector2(0.5f, 0.5f);
            powerUpsRect.anchoredPosition = new Vector2(-96.37988f, 50);
            powerUpsRect.sizeDelta = new Vector2(192.77f, 100);
            powerUpsRect.localScale = Vector3.one;
            powerUpsRect.localRotation = Quaternion.identity;
            var layout = powerUps.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(0, 0, 0, 15);
                layout.spacing = 15;
                layout.childAlignment = TextAnchor.LowerCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }
        }

        GameObject returnButton = FindSceneObject("ReturnButton");
        if (returnButton != null)
            returnButton.SetActive(false);
    }

    private static string ValidateInternal()
    {
        string[] textureNames =
        {
            "HudStageProgressShell.png", "HudKnifeMagazineShell.png", "HudPowerUpBezel.png",
            "HudCountBadge.png", "HudProgressMarker.png"
        };
        bool texturesOk = textureNames.All(name => AssetDatabase.LoadAssetAtPath<Sprite>(HudArt + name) != null);

        GameObject stage = FindSceneObject("StageProgressBar");
        GameObject magazine = FindSceneObject("KnifeMagazine");
        GameObject currency = FindSceneObject("CoinUiDisaply");
        bool stageLinked = IsLinkedTo(stage, StagePrefabPath);
        bool magazineLinked = IsLinkedTo(magazine, MagazinePrefabPath);
        bool currencyLinked = IsLinkedTo(currency, CurrencyDisplayPath);
        bool sharedSourcesExist = AssetDatabase.LoadAssetAtPath<GameObject>(ProgressSlotPath) != null &&
            AssetDatabase.LoadAssetAtPath<GameObject>(KnifeIconPath) != null &&
            AssetDatabase.LoadAssetAtPath<GameObject>(PowerUpButtonPath) != null;

        int decorativeRaycastTargets = 0;
        if (stage != null)
            decorativeRaycastTargets += stage.GetComponentsInChildren<Graphic>(true).Count(g => g.raycastTarget);
        if (magazine != null)
            decorativeRaycastTargets += magazine.GetComponentsInChildren<Graphic>(true).Count(g => g.raycastTarget);

        bool ok = texturesOk && stageLinked && magazineLinked && currencyLinked &&
            sharedSourcesExist && decorativeRaycastTargets == 0;
        return $"Gameplay HUD validation {(ok ? "PASS" : "FAIL")} | sprites={texturesOk} | " +
            $"stagePrefabLinked={stageLinked} | magazinePrefabLinked={magazineLinked} | " +
            $"currencyPrefabLinked={currencyLinked} | " +
            $"sharedChildSources={sharedSourcesExist} | decorativeRaycastTargets={decorativeRaycastTargets}";
    }

    private static bool IsLinkedTo(GameObject instance, string prefabPath)
    {
        if (instance == null || !PrefabUtility.IsPartOfPrefabInstance(instance))
            return false;
        GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance);
        return source != null && AssetDatabase.GetAssetPath(source) == prefabPath;
    }

    private static void WireAnimatorGraphic(GameObject root, Graphic graphic)
    {
        Component animator = root.GetComponent("UiElementAnimator");
        if (animator == null)
            return;
        SetObjectReference(animator, "_RectTransform", root.GetComponent<RectTransform>());
        SetObjectReference(animator, "_Graphic", graphic);
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float size,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject child = CreateUiObject(name, parent);
        var rect = child.GetComponent<RectTransform>();
        SetStretch(rect, anchorMin, anchorMax);
        var label = child.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);
        var shadow = child.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.05f, 0.025f, 0.01f, 0.86f);
        shadow.effectDistance = new Vector2(2.2f, -2.2f);
        shadow.useGraphicAlpha = true;
        return label;
    }

    private static void StyleText(TMP_Text text, float size, Color color)
    {
        if (text == null)
            return;
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        var shadow = text.GetComponent<Shadow>() ?? text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.58f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.layer = 5;
        child.transform.SetParent(parent, false);
        return child;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static void SetStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void SetCorner(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size,
        Vector2? pivot = null)
    {
        if (rect == null)
            return;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
            throw new MissingFieldException(target.GetType().Name, propertyName);
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetColor(UnityEngine.Object target, string propertyName, Color value)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
            throw new MissingFieldException(target.GetType().Name, propertyName);
        property.colorValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void UnpackRootIfNeeded(GameObject root)
    {
        if (PrefabUtility.IsPartOfPrefabInstance(root))
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
            SetLayerRecursively(root.GetChild(i), layer);
    }

    private static Sprite LoadSprite(string fileName)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HudArt + fileName);
        if (sprite == null)
            throw new InvalidOperationException($"Missing HUD sprite: {HudArt + fileName}");
        return sprite;
    }

    private static Sprite LoadSpriteByName(string assetPath, string spriteName)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .FirstOrDefault(candidate => candidate.name == spriteName);
        if (sprite == null)
            throw new InvalidOperationException($"Missing sprite '{spriteName}' in {assetPath}");
        return sprite;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static GameObject FindSceneObject(string name)
    {
        var scene = EditorSceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform match = root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == name);
            if (match != null)
                return match.gameObject;
        }
        return null;
    }
}
#endif
