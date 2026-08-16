#if UNITY_EDITOR
using System;
using System.Linq;
using Common.Ads.Data;
using Common.Events;
using DG.Tweening;
using Game.Ads.Data;
using Game.Economy.Data;
using Game.Installers;
using Game.UI.Common;
using Game.UI.Popups;
using Game.UI.Store;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;

public static class StageFailPopupProductionBuilder
{
    private const string StageScenePath = "Assets/Scenes/StagePlay.unity";
    private const string ActionButtonPrefabPath = "Assets/Prefabs/UI/Buttons/FailureActionButton.prefab";
    private const string PricePillPrefabPath = "Assets/Prefabs/UI/Store/PricePillView.prefab";
    private const string PopupTemplatePath = "Assets/Prefabs/UI/PopupTemplate.prefab";
    private const string ShellSpritePath = "Assets/Art/UI/Failure/NeutralGameOverShell_Source.png";
    private const string ShellShaderPath = "Assets/Shaders/UI/GeneratedCheckerboardKey.shader";
    private const string ShellMaterialPath = "Assets/Art/UI/Failure/NeutralGameOverShell.mat";
    private const string RetryButtonSpritePath = "Assets/Art/UI/Failure/FailureRetryButton_Source.png";
    private const string KnifeAnimationAtlasPath = "Assets/Art/UI/Failure/FailureKnivesAnimation.png";
    private const string PrimaryButtonSpritePath = "Assets/Art/UI/Core/ButtonPrimary.png";
    private const string SecondaryButtonSpritePath = "Assets/Art/UI/Core/ButtonSecondary.png";
    private const string HudFontPath = "Assets/Art/UI/Fonts/BarlowCondensed-ExtraBold SDF.asset";
    private const string ImpactGlowPath = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Particles/Texture/fx_glow_small.png";
    private const string ContinueRequestedChannelPath = "Assets/Configurations/Events/Ads/ContinueRequestedChannel.asset";
    private const string ContinuePlacementPath = "Assets/Configurations/Systems/Ads/RewardPlacement/ContinuePlacement.asset";
    private const string InterstitialPlacementPath = "Assets/Configurations/Systems/Ads/RewardPlacement/InterstitialPlacement.asset";
    private const int KnifeAnimationFrameCount = 16;

    [MenuItem("Tools/Toss Madness/UI/Build Approved Failure Popup")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Build the failure popup outside Play Mode.");

        EnsureFolder("Assets/Prefabs/UI/Buttons");
        EnsureFolder("Assets/Art/UI/Failure");
        bool importerChanged = ConfigureShellImporter();
        importerChanged |= ConfigureRetryButtonImporter();
        importerChanged |= ConfigureKnifeAnimationImporter();
        if (importerChanged)
        {
            EditorApplication.delayCall += Build;
            return;
        }
        BuildActionButtonPrefab();
        EnsureStageSceneOpen();
        UpdateScene();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(ValidateInternal());
    }

    [MenuItem("Tools/Toss Madness/UI/Validate Approved Failure Popup")]
    public static void Validate()
    {
        Debug.Log(ValidateInternal());
    }

    private static bool ConfigureShellImporter()
    {
        return ConfigureGeneratedSpriteImporter(ShellSpritePath, "approved neutral shell", false);
    }

    private static bool ConfigureRetryButtonImporter()
    {
        return ConfigureGeneratedSpriteImporter(RetryButtonSpritePath, "approved compound Retry button", true);
    }

    private static bool ConfigureGeneratedSpriteImporter(
        string assetPath,
        string assetDescription,
        bool alphaIsTransparency)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Missing {assetDescription} source at {assetPath}.");

        bool importerChanged = importer.textureType != TextureImporterType.Sprite ||
                               importer.spriteImportMode != SpriteImportMode.Single ||
                               importer.alphaIsTransparency != alphaIsTransparency ||
                               importer.mipmapEnabled ||
                               importer.filterMode != FilterMode.Bilinear ||
                               importer.textureCompression != TextureImporterCompression.Uncompressed ||
                               importer.maxTextureSize != 2048;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = alphaIsTransparency;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 100f;
        if (!importerChanged)
            return false;
        importer.SaveAndReimport();
        return true;
    }

    private static SpriteRect SpriteData(string name, Rect rect, SpriteRect[] existing) => new SpriteRect
    {
        name = name,
        rect = rect,
        alignment = SpriteAlignment.Center,
        pivot = new Vector2(0.5f, 0.5f),
        border = Vector4.zero,
        spriteID = existing.FirstOrDefault(sprite => sprite.name == name)?.spriteID ?? GUID.Generate(),
    };

    private static bool ConfigureKnifeAnimationImporter()
    {
        TextureImporter importer = AssetImporter.GetAtPath(KnifeAnimationAtlasPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Missing failure knife animation atlas at {KnifeAnimationAtlasPath}.");

        bool importerChanged = importer.textureType != TextureImporterType.Sprite ||
                               importer.spriteImportMode != SpriteImportMode.Multiple ||
                               !importer.alphaIsTransparency ||
                               importer.mipmapEnabled ||
                               importer.filterMode != FilterMode.Bilinear ||
                               importer.textureCompression != TextureImporterCompression.Uncompressed ||
                               importer.maxTextureSize != 2048;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 100f;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        SpriteRect[] existing = dataProvider.GetSpriteRects();
        SpriteRect[] desired = Enumerable.Range(0, KnifeAnimationFrameCount)
            .Select(index => SpriteData(
                KnifeFrameName(index),
                new Rect((index % 4) * 512f, (3 - index / 4) * 512f, 512f, 512f),
                existing))
            .ToArray();
        bool rectsChanged = existing.Length != desired.Length || desired.Any(expected =>
            existing.All(current => current.name != expected.name || current.rect != expected.rect));
        if (!importerChanged && !rectsChanged)
            return false;

        dataProvider.SetSpriteRects(desired);
        ISpriteNameFileIdDataProvider nameProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider?.SetNameFileIdPairs(desired
            .Select(sprite => new SpriteNameFileIdPair(sprite.name, sprite.spriteID))
            .ToArray());
        dataProvider.Apply();
        importer.SaveAndReimport();
        return true;
    }

    private static void BuildActionButtonPrefab()
    {
        bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(ActionButtonPrefabPath) != null;
        GameObject root = prefabExists
            ? PrefabUtility.LoadPrefabContents(ActionButtonPrefabPath)
            : new GameObject("FailureActionButton", typeof(RectTransform));

        try
        {
            root.name = "FailureActionButton";
            root.layer = 5;
            ClearChildren(root.transform);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(700f, 165f);
            rootRect.localScale = Vector3.one;

            Image hitArea = GetOrAdd<Image>(root);
            hitArea.sprite = null;
            hitArea.type = Image.Type.Simple;
            hitArea.color = new Color(1f, 1f, 1f, 0.001f);
            hitArea.raycastTarget = true;

            Button button = GetOrAdd<Button>(root);
            button.targetGraphic = hitArea;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            CanvasGroup interactionGroup = GetOrAdd<CanvasGroup>(root);
            interactionGroup.alpha = 1f;
            interactionGroup.interactable = true;
            interactionGroup.blocksRaycasts = true;

            GameObject visualRootObject = CreateUiObject("VisualRoot", root.transform);
            RectTransform visualRoot = visualRootObject.GetComponent<RectTransform>();
            Stretch(visualRoot, Vector2.zero, Vector2.zero);
            Image visual = visualRootObject.AddComponent<Image>();
            visual.sprite = LoadSpriteAtPath(PrimaryButtonSpritePath);
            visual.type = Image.Type.Simple;
            visual.preserveAspect = true;
            visual.color = Color.white;
            visual.raycastTarget = false;

            TMP_Text label = CreateLabel(
                "Label",
                visualRoot,
                "RETRY",
                58f,
                new Color(0.12f, 0.075f, 0.035f, 1f),
                TextAlignmentOptions.Center,
                false);
            Stretch(label.rectTransform, new Vector2(24f, 18f), new Vector2(-24f, -12f));

            ActionButtonPresenter presenter = GetOrAdd<ActionButtonPresenter>(root);
            SerializedObject presenterObject = new SerializedObject(presenter);
            SetReference(presenterObject, "_Button", button);
            SetReference(presenterObject, "_PresentationRoot", visualRoot);
            SetReference(presenterObject, "_InteractionGroup", interactionGroup);
            SetReference(presenterObject, "_HitAreaGraphic", hitArea);
            SetReference(presenterObject, "_SurfaceImage", visual);
            presenterObject.FindProperty("_SkinStyle").enumValueIndex = (int)Game.UI.Tokens.UiButtonStyle.Unskinned;
            presenterObject.ApplyModifiedPropertiesWithoutUndo();

            TouchTargetPadding touchTargetPadding = GetOrAdd<TouchTargetPadding>(root);
            touchTargetPadding.EnsureHitArea();

            ConfigureElementAnimator(
                root,
                rootRect,
                visual,
                new Vector2(0f, -24f),
                new Vector3(0.9f, 0.9f, 1f),
                Vector3.zero,
                0.3f,
                0.34f,
                true);

            PrefabUtility.SaveAsPrefabAsset(root, ActionButtonPrefabPath);
        }
        finally
        {
            if (prefabExists)
                PrefabUtility.UnloadPrefabContents(root);
            else
                UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void UpdateScene()
    {
        StageFailPopup popup = UnityEngine.Object.FindObjectsByType<StageFailPopup>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Single();

        RectTransform popupRect = popup.GetComponent<RectTransform>();
        Stretch(popupRect, Vector2.zero, Vector2.zero);
        PrefabUtility.RecordPrefabInstancePropertyModifications(popupRect);

        Transform blackBackground = popup.transform.Find("BlackBackground");
        Transform holder = popup.transform.Find("Holder");
        Transform content = popup.transform.Find("Holder/Content");
        if (blackBackground == null || holder == null || content == null)
            throw new InvalidOperationException("StageFailPopup no longer matches PopupTemplate structure.");

        Image scrim = blackBackground.GetComponent<Image>();
        scrim.color = new Color(0f, 0f, 0f, 0.82f);
        scrim.raycastTarget = true;
        PrefabUtility.RecordPrefabInstancePropertyModifications(scrim);

        RectTransform holderRect = (RectTransform)holder;
        holderRect.anchorMin = holderRect.anchorMax = holderRect.pivot = new Vector2(0.5f, 0.5f);
        holderRect.anchoredPosition = new Vector2(0f, 30f);
        holderRect.sizeDelta = new Vector2(1050f, 900f);
        holderRect.localScale = Vector3.one;
        PrefabUtility.RecordPrefabInstancePropertyModifications(holderRect);

        Image holderImage = holder.GetComponent<Image>();
        holderImage.sprite = null;
        holderImage.material = null;
        holderImage.type = Image.Type.Simple;
        holderImage.preserveAspect = false;
        holderImage.color = Color.clear;
        holderImage.raycastTarget = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(holderImage);

        RectTransform contentRect = (RectTransform)content;
        Stretch(contentRect, Vector2.zero, Vector2.zero);
        PrefabUtility.RecordPrefabInstancePropertyModifications(contentRect);
        ClearChildren(content);

        Image shell = CreateImage("NeutralGameOverShell", content, LoadSpriteAtPath(ShellSpritePath));
        PlaceCenter(shell.rectTransform, new Vector2(0f, 40f), new Vector2(1050f, 727f));
        shell.material = EnsureShellMaterial();
        shell.preserveAspect = true;

        TMP_Text stageLevelLabel = CreateStageLevelLabel(content);
        PlaceCenter(stageLevelLabel.rectTransform, new Vector2(0f, 326f), new Vector2(640f, 58f));
        ConfigureElementAnimator(
            stageLevelLabel.gameObject,
            stageLevelLabel.rectTransform,
            stageLevelLabel,
            new Vector2(0f, -18f),
            new Vector3(0.92f, 0.92f, 1f),
            Vector3.zero,
            0.24f,
            0.04f,
            false);

        TMP_Text title = CreateTitleLabel(content);
        PlaceCenter(title.rectTransform, new Vector2(0f, 250f), new Vector2(780f, 125f));
        ConfigureElementAnimator(
            title.gameObject,
            title.rectTransform,
            title,
            new Vector2(0f, -30f),
            new Vector3(0.88f, 0.88f, 1f),
            new Vector3(0f, 0f, -1.5f),
            0.3f,
            0.1f,
            false);

        Image impactGlow = CreateImage("ImpactGlow", content, LoadSpriteAtPath(ImpactGlowPath));
        PlaceCenter(impactGlow.rectTransform, new Vector2(0f, 35f), new Vector2(450f, 450f));
        impactGlow.color = new Color(1f, 0.28f, 0.04f, 0.18f);
        ConfigureElementAnimator(
            impactGlow.gameObject,
            impactGlow.rectTransform,
            impactGlow,
            Vector2.zero,
            new Vector3(0.7f, 0.7f, 1f),
            Vector3.zero,
            0.24f,
            0.07f,
            true);
        ConfigureHeartbeat(impactGlow.gameObject, 0.8f, 1.7f, 1.04f, 1.018f);

        Sprite[] knifeFrames = LoadKnifeAnimationFrames();
        Image knives = CreateImage("CrossedKnives", content, knifeFrames[0]);
        PlaceCenter(knives.rectTransform, new Vector2(0f, 35f), new Vector2(390f, 390f));
        FailureKnifeSpriteAnimator knifeSpriteAnimator = knives.gameObject.AddComponent<FailureKnifeSpriteAnimator>();
        SerializedObject knifeAnimatorObject = new SerializedObject(knifeSpriteAnimator);
        SetReference(knifeAnimatorObject, "_TargetImage", knives);
        SerializedProperty framesProperty = knifeAnimatorObject.FindProperty("_Frames");
        framesProperty.arraySize = knifeFrames.Length;
        for (int index = 0; index < knifeFrames.Length; index++)
            framesProperty.GetArrayElementAtIndex(index).objectReferenceValue = knifeFrames[index];
        knifeAnimatorObject.FindProperty("_StartDelay").floatValue = 0.08f;
        knifeAnimatorObject.FindProperty("_SecondsPerFrame").floatValue = 0.028f;
        knifeAnimatorObject.FindProperty("_UseUnscaledTime").boolValue = true;
        knifeAnimatorObject.ApplyModifiedPropertiesWithoutUndo();
        ConfigureElementAnimator(
            knives.gameObject,
            knives.rectTransform,
            knives,
            Vector2.zero,
            new Vector3(0.88f, 0.88f, 1f),
            new Vector3(0f, 0f, -2f),
            0.18f,
            0.08f,
            true);
        ConfigureHeartbeat(knives.gameObject, 1.05f, 2.1f, 1.016f, 1.007f);

        GameObject buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ActionButtonPrefabPath);
        if (buttonPrefab == null)
            throw new InvalidOperationException($"Missing shared action button prefab: {ActionButtonPrefabPath}");

        GameObject retryObject = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, content);
        retryObject.name = "RetryButton";
        RectTransform retryRect = retryObject.GetComponent<RectTransform>();
        PlaceCenter(retryRect, new Vector2(0f, -238f), new Vector2(730f, 205f));
        ConfigureButtonInstance(
            retryObject,
            RetryButtonSpritePath,
            "RETRY",
            new Color(0.105f, 0.045f, 0.012f, 1f),
            new Vector2(44f, 22f),
            new Vector2(-266f, -14f));
        Image retrySurface = retryObject.transform.Find("VisualRoot").GetComponent<Image>();
        retrySurface.material = null;
        PrefabUtility.RecordPrefabInstancePropertyModifications(retrySurface);

        TMP_Text retryLabel = retryObject.transform.Find("VisualRoot/Label").GetComponent<TMP_Text>();
        retryLabel.fontSize = 78f;
        retryLabel.characterSpacing = 2f;
        PrefabUtility.RecordPrefabInstancePropertyModifications(retryLabel);
        ConfigureElementAnimator(
            retryObject,
            retryRect,
            retrySurface,
            new Vector2(0f, -24f),
            new Vector3(0.9f, 0.9f, 1f),
            Vector3.zero,
            0.3f,
            0.34f,
            true);

        GameObject pricePillPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PricePillPrefabPath);
        if (pricePillPrefab == null)
            throw new InvalidOperationException($"Missing shared price pill prefab: {PricePillPrefabPath}");

        Transform retryVisualRoot = retryObject.transform.Find("VisualRoot");
        GameObject retryPriceObject = (GameObject)PrefabUtility.InstantiatePrefab(pricePillPrefab, retryVisualRoot);
        retryPriceObject.name = "RetryPricePill";
        RectTransform retryPriceRect = retryPriceObject.GetComponent<RectTransform>();
        retryPriceRect.anchorMin = retryPriceRect.anchorMax = retryPriceRect.pivot = new Vector2(0.5f, 0.5f);
        retryPriceRect.anchoredPosition = new Vector2(178f, 0f);
        retryPriceRect.sizeDelta = new Vector2(200f, 78f);
        retryPriceRect.localScale = Vector3.one;
        StorePricePillView retryPricePill = retryPriceObject.GetComponent<StorePricePillView>();
        retryPricePill.SetPrice(50, CurrencyType.Coin);
        retryPricePill.FillImage.gameObject.SetActive(false);
        retryPricePill.BorderImage.gameObject.SetActive(false);
        RectTransform retryPriceContent = retryPriceObject.transform.Find("Content") as RectTransform;
        retryPriceContent.sizeDelta = new Vector2(-24f, -12f);

        GameObject menuObject = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, content);
        menuObject.name = "MenuButton";
        RectTransform menuRect = menuObject.GetComponent<RectTransform>();
        PlaceCenter(menuRect, new Vector2(0f, -398f), new Vector2(400f, 112f));
        ConfigureButtonInstance(
            menuObject,
            SecondaryButtonSpritePath,
            "MENU",
            new Color(1f, 0.96f, 0.87f, 1f),
            new Vector2(18f, 10f),
            new Vector2(-18f, -8f));
        ConfigureElementAnimator(
            menuObject,
            menuRect,
            menuObject.transform.Find("VisualRoot").GetComponent<Image>(),
            new Vector2(0f, -18f),
            new Vector3(0.92f, 0.92f, 1f),
            Vector3.zero,
            0.26f,
            0.44f,
            false);

        SerializedObject popupObject = new SerializedObject(popup);
        SetReference(popupObject, "_StageLevelLabel", stageLevelLabel);
        SetReference(popupObject, "_RetryButton", retryObject.GetComponent<Button>());
        SetReference(popupObject, "_RetryPricePill", retryPricePill);
        SetReference(popupObject, "_MenuButton", menuObject.GetComponent<Button>());
        SetReference(
            popupObject,
            "_ContinueRequestedChannel",
            AssetDatabase.LoadAssetAtPath<VoidEventChannel>(ContinueRequestedChannelPath));
        popupObject.ApplyModifiedPropertiesWithoutUndo();

        StageInstaller stageInstaller = UnityEngine.Object.FindFirstObjectByType<StageInstaller>(FindObjectsInactive.Include);
        if (stageInstaller == null)
            throw new InvalidOperationException("StagePlay is missing StageInstaller.");

        SerializedObject installerObject = new SerializedObject(stageInstaller);
        SetReference(
            installerObject,
            "_ContinueRequestedChannel",
            AssetDatabase.LoadAssetAtPath<VoidEventChannel>(ContinueRequestedChannelPath));
        SetReference(
            installerObject,
            "_ContinuePlacement",
            AssetDatabase.LoadAssetAtPath<RewardedPlacement>(ContinuePlacementPath));
        SetReference(
            installerObject,
            "_InterstitialPlacement",
            AssetDatabase.LoadAssetAtPath<InterstitialPlacement>(InterstitialPlacementPath));
        installerObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(popup);
        EditorUtility.SetDirty(stageInstaller);
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        GameObject gameObject = CreateUiObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateStageLevelLabel(Transform parent)
    {
        return CreateLabel(
            "StageLevelLabel",
            parent,
            "STAGE 1  •  LEVEL 1",
            42f,
            new Color(1f, 0.96f, 0.87f, 1f),
            TextAlignmentOptions.Center,
            true);
    }

    private static TMP_Text CreateTitleLabel(Transform parent)
    {
        TMP_Text label = CreateLabel(
            "GameOverTitle",
            parent,
            "GAME OVER",
            104f,
            new Color(1f, 0.63f, 0.08f, 1f),
            TextAlignmentOptions.Center,
            true);
        Outline outline = label.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.12f, 0.055f, 0.018f, 0.95f);
        outline.effectDistance = new Vector2(4f, -5f);
        outline.useGraphicAlpha = true;
        return label;
    }

    private static TMP_Text CreateLabel(
        string name,
        Transform parent,
        string text,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        bool addShadow)
    {
        GameObject gameObject = CreateUiObject(name, parent);
        TextMeshProUGUI label = gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HudFontPath);
        if (label.font == null)
            throw new InvalidOperationException($"Missing HUD font at {HudFontPath}.");
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = alignment;
        label.color = color;
        label.characterSpacing = 1f;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;

        if (addShadow)
        {
            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.035f, 0.012f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -4f);
            shadow.useGraphicAlpha = true;
        }

        return label;
    }

    private static void ConfigureButtonInstance(
        GameObject buttonObject,
        string spritePath,
        string labelText,
        Color labelColor,
        Vector2 labelOffsetMin,
        Vector2 labelOffsetMax)
    {
        Image visual = buttonObject.transform.Find("VisualRoot")?.GetComponent<Image>();
        if (visual == null)
            throw new InvalidOperationException($"{buttonObject.name} lost its shared VisualRoot image.");

        visual.sprite = LoadSpriteAtPath(spritePath);
        visual.preserveAspect = true;
        visual.color = spritePath == SecondaryButtonSpritePath
            ? new Color(0.42f, 0.72f, 0.68f, 1f)
            : Color.white;
        PrefabUtility.RecordPrefabInstancePropertyModifications(visual);

        TMP_Text label = visual.transform.Find("Label")?.GetComponent<TMP_Text>();
        if (label == null)
            throw new InvalidOperationException($"{buttonObject.name} lost its shared live Label.");
        label.text = labelText;
        label.color = labelColor;
        Stretch(label.rectTransform, labelOffsetMin, labelOffsetMax);
        PrefabUtility.RecordPrefabInstancePropertyModifications(label);
    }

    private static Material EnsureShellMaterial()
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShellShaderPath);
        if (shader == null)
            throw new InvalidOperationException($"Missing shell transparency shader at {ShellShaderPath}.");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(ShellMaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "NeutralGameOverShell" };
            AssetDatabase.CreateAsset(material, ShellMaterialPath);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    private static void ConfigureElementAnimator(
        GameObject gameObject,
        RectTransform rectTransform,
        Graphic graphic,
        Vector2 positionOffset,
        Vector3 scaleMultiplier,
        Vector3 rotationOffset,
        float duration,
        float delay,
        bool heartbeat)
    {
        GetOrAdd<CanvasGroup>(gameObject);
        UiElementAnimator animator = GetOrAdd<UiElementAnimator>(gameObject);
        SerializedObject animatorObject = new SerializedObject(animator);
        SetReference(animatorObject, "_RectTransform", rectTransform);
        SetReference(animatorObject, "_Graphic", graphic);
        animatorObject.FindProperty("_PlayInOnEnable").boolValue = true;
        animatorObject.FindProperty("_UseUnscaledTime").boolValue = true;
        animatorObject.FindProperty("_EnableGameObjectOnPlayIn").boolValue = true;
        animatorObject.FindProperty("_DisableGameObjectOnPlayOut").boolValue = false;

        SerializedProperty inSettings = animatorObject.FindProperty("_In");
        inSettings.FindPropertyRelative("_PositionOffset").vector2Value = positionOffset;
        inSettings.FindPropertyRelative("_ScaleMultiplier").vector3Value = scaleMultiplier;
        inSettings.FindPropertyRelative("_RotationOffset").vector3Value = rotationOffset;
        inSettings.FindPropertyRelative("_Color").colorValue = Color.white;
        inSettings.FindPropertyRelative("_Alpha").floatValue = 0f;
        inSettings.FindPropertyRelative("_Duration").floatValue = duration;
        inSettings.FindPropertyRelative("_Delay").floatValue = delay;
        inSettings.FindPropertyRelative("_Ease").intValue = (int)Ease.OutBack;

        SerializedProperty idle = animatorObject.FindProperty("_IdleAnimation");
        idle.FindPropertyRelative("_Mode").intValue = heartbeat
            ? (int)UiElementAnimator.IdleAnimationMode.Heartbeat
            : (int)UiElementAnimator.IdleAnimationMode.None;
        idle.FindPropertyRelative("_PrimaryScaleMultiplier").floatValue = 1.025f;
        idle.FindPropertyRelative("_SecondaryScaleMultiplier").floatValue = 1.012f;
        idle.FindPropertyRelative("_UpDuration").floatValue = 0.12f;
        idle.FindPropertyRelative("_DownDuration").floatValue = 0.1f;
        idle.FindPropertyRelative("_SecondaryUpDuration").floatValue = 0.09f;
        idle.FindPropertyRelative("_SecondaryDownDuration").floatValue = 0.08f;
        idle.FindPropertyRelative("_BetweenLoopsDelay").floatValue = 2.4f;
        idle.FindPropertyRelative("_InitialDelay").floatValue = 1.2f;
        animatorObject.ApplyModifiedPropertiesWithoutUndo();

        if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
    }

    private static void ConfigureHeartbeat(
        GameObject gameObject,
        float initialDelay,
        float betweenLoopsDelay,
        float primaryScale,
        float secondaryScale)
    {
        UiElementAnimator animator = gameObject.GetComponent<UiElementAnimator>();
        if (animator == null)
            throw new InvalidOperationException($"{gameObject.name} needs UiElementAnimator before heartbeat tuning.");

        SerializedObject animatorObject = new SerializedObject(animator);
        SerializedProperty idle = animatorObject.FindProperty("_IdleAnimation");
        idle.FindPropertyRelative("_Mode").intValue = (int)UiElementAnimator.IdleAnimationMode.Heartbeat;
        idle.FindPropertyRelative("_InitialDelay").floatValue = initialDelay;
        idle.FindPropertyRelative("_BetweenLoopsDelay").floatValue = betweenLoopsDelay;
        idle.FindPropertyRelative("_PrimaryScaleMultiplier").floatValue = primaryScale;
        idle.FindPropertyRelative("_SecondaryScaleMultiplier").floatValue = secondaryScale;
        animatorObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static string ValidateInternal()
    {
        if (EditorSceneManager.GetActiveScene().path != StageScenePath)
            return "Failure popup validation requires Assets/Scenes/StagePlay.unity to be open.";

        StageFailPopup popup = UnityEngine.Object.FindObjectsByType<StageFailPopup>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .SingleOrDefault();
        if (popup == null)
            return "FAIL: StageFailPopup was not found.";

        TMPro.TMP_Text[] runtimeLabels = popup.GetComponentsInChildren<TMPro.TMP_Text>(true);
        TMPro.TMP_Text stageLevelLabel = runtimeLabels.SingleOrDefault(label => label.name == "StageLevelLabel");
        TMPro.TMP_Text titleLabel = runtimeLabels.SingleOrDefault(label => label.name == "GameOverTitle");
        StorePricePillView retryPricePill = popup.GetComponentInChildren<StorePricePillView>(true);
        if (runtimeLabels.Length != 5 || stageLevelLabel == null || titleLabel == null || retryPricePill?.AmountText == null)
            return "FAIL: popup must contain live progress, title, two action labels, and Retry cost text.";
        if (runtimeLabels.Where(label => label != retryPricePill.AmountText)
            .Any(label => AssetDatabase.GetAssetPath(label.font) != HudFontPath))
            return $"FAIL: all popup-owned typography must reuse the production font at {HudFontPath}.";
        if (titleLabel.text != "GAME OVER")
            return "FAIL: failure title must be the exact live label GAME OVER.";
        GameObject pricePillSource = PrefabUtility.GetCorrespondingObjectFromSource(retryPricePill.gameObject);
        if (pricePillSource == null || AssetDatabase.GetAssetPath(pricePillSource) != PricePillPrefabPath)
            return $"FAIL: Retry cost must retain its nested shared prefab link to {PricePillPrefabPath}.";
        if (retryPricePill.FillImage.gameObject.activeSelf || retryPricePill.BorderImage.gameObject.activeSelf)
            return "FAIL: Retry price content must sit directly in the generated inset pocket without a second pill surface.";

        Image[] popupImages = popup.GetComponentsInChildren<Image>(true);
        Image shell = popupImages.SingleOrDefault(image => image.name == "NeutralGameOverShell");
        if (shell == null || AssetDatabase.GetAssetPath(shell.sprite) != ShellSpritePath)
            return $"FAIL: popup must use the approved world-neutral shell at {ShellSpritePath}.";
        if (AssetDatabase.GetAssetPath(shell.material) != ShellMaterialPath)
            return $"FAIL: neutral shell must use the recovered-alpha material at {ShellMaterialPath}.";

        Image impactGlow = popupImages.SingleOrDefault(image => image.name == "ImpactGlow");
        if (impactGlow == null || AssetDatabase.GetAssetPath(impactGlow.sprite) != ImpactGlowPath)
            return $"FAIL: popup must contain the non-interactive ImpactGlow sourced from {ImpactGlowPath}.";
        if (impactGlow.raycastTarget)
            return "FAIL: ImpactGlow must not block pointer input.";

        if (popupImages.Any(image => image.sprite != null &&
                                     AssetDatabase.GetAssetPath(image.sprite) == "Assets/Art/UI/Failure/FailurePopupAtlas.png"))
            return "FAIL: rejected baked title/action artwork is still present in the popup.";

        FailureKnifeSpriteAnimator knifeAnimator = popup.GetComponentInChildren<FailureKnifeSpriteAnimator>(true);
        if (knifeAnimator == null)
            return "FAIL: popup is missing the one-shot knife sprite animator.";

        SerializedObject knifeAnimatorObject = new SerializedObject(knifeAnimator);
        SerializedProperty frames = knifeAnimatorObject.FindProperty("_Frames");
        if (frames == null || frames.arraySize != KnifeAnimationFrameCount)
            return $"FAIL: knife animation must contain exactly {KnifeAnimationFrameCount} frames.";
        for (int index = 0; index < KnifeAnimationFrameCount; index++)
        {
            Sprite frame = frames.GetArrayElementAtIndex(index).objectReferenceValue as Sprite;
            if (frame == null || AssetDatabase.GetAssetPath(frame) != KnifeAnimationAtlasPath || frame.name != KnifeFrameName(index))
                return $"FAIL: knife animation frame {index} is not wired to {KnifeFrameName(index)}.";
        }

        Button[] buttons = popup.GetComponentsInChildren<Button>(true);
        if (buttons.Length != 2 || buttons.All(button => button.name != "RetryButton") || buttons.All(button => button.name != "MenuButton"))
            return "FAIL: popup must contain exactly the linked RetryButton and MenuButton instances.";

        foreach (Button button in buttons)
        {
            TouchTargetPadding padding = button.GetComponent<TouchTargetPadding>();
            if (padding == null || padding.MinimumSizeDp < 48f)
                return $"FAIL: {button.name} must retain the shared 48 dp touch-target padding.";

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(button.gameObject);
            if (source == null || AssetDatabase.GetAssetPath(source) != ActionButtonPrefabPath)
                return $"FAIL: {button.name} is not linked to {ActionButtonPrefabPath}.";

            Image surface = button.transform.Find("VisualRoot")?.GetComponent<Image>();
            TMP_Text label = button.transform.Find("VisualRoot/Label")?.GetComponent<TMP_Text>();
            string expectedSprite = button.name == "RetryButton" ? RetryButtonSpritePath : SecondaryButtonSpritePath;
            string expectedLabel = button.name == "RetryButton" ? "RETRY" : "MENU";
            if (surface == null || AssetDatabase.GetAssetPath(surface.sprite) != expectedSprite || label?.text != expectedLabel)
                return $"FAIL: {button.name} lost its approved shared surface or live action label.";
        }

        GameObject popupSource = PrefabUtility.GetCorrespondingObjectFromSource(popup.gameObject);
        if (popupSource == null || AssetDatabase.GetAssetPath(popupSource) != PopupTemplatePath)
            return $"FAIL: StageFailPopup is not linked to {PopupTemplatePath}.";

        SerializedObject popupObject = new SerializedObject(popup);
        if (popupObject.FindProperty("_StageLevelLabel")?.objectReferenceValue == null ||
            popupObject.FindProperty("_RetryButton")?.objectReferenceValue == null ||
            popupObject.FindProperty("_RetryPricePill")?.objectReferenceValue == null ||
            popupObject.FindProperty("_MenuButton")?.objectReferenceValue == null ||
            popupObject.FindProperty("_ContinueRequestedChannel")?.objectReferenceValue == null)
            return "FAIL: StageFailPopup progress/paid-retry/action bindings are incomplete.";

        StageInstaller stageInstaller = UnityEngine.Object.FindFirstObjectByType<StageInstaller>(FindObjectsInactive.Include);
        SerializedObject installerObject = stageInstaller != null ? new SerializedObject(stageInstaller) : null;
        if (installerObject == null ||
            installerObject.FindProperty("_ContinueRequestedChannel")?.objectReferenceValue == null ||
            installerObject.FindProperty("_ContinuePlacement")?.objectReferenceValue == null ||
            installerObject.FindProperty("_InterstitialPlacement")?.objectReferenceValue == null)
            return "FAIL: StageInstaller paid-retry/interstitial bindings are incomplete.";

        return "PASS: approved world-neutral Failure popup uses live stage/level and title typography, the configured coin-cost Retry transaction with shared price content inside the compound Retry surface, the new charcoal/teal/gold shell, a sixteen-frame independent-knife impact, restrained ambient glow/settle motion, two shared-prefab actions, valid recovery bindings, and PopupTemplate linkage.";
    }

    private static void EnsureStageSceneOpen()
    {
        if (EditorSceneManager.GetActiveScene().path == StageScenePath)
            return;

        if (EditorSceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save or discard the current scene before building the failure popup.");

        EditorSceneManager.OpenScene(StageScenePath, OpenSceneMode.Single);
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }

    private static void ClearChildren(Transform parent)
    {
        for (int index = parent.childCount - 1; index >= 0; index--)
            UnityEngine.Object.DestroyImmediate(parent.GetChild(index).gameObject);
    }

    private static string KnifeFrameName(int index) => $"FailureKnives_{index:00}";

    private static Sprite[] LoadKnifeAnimationFrames()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(KnifeAnimationAtlasPath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();
        if (sprites.Length != KnifeAnimationFrameCount ||
            sprites.Where((sprite, index) => sprite.name != KnifeFrameName(index)).Any())
            throw new InvalidOperationException(
                $"{KnifeAnimationAtlasPath} must contain {KnifeAnimationFrameCount} ordered animation frames.");
        return sprites;
    }

    private static Sprite LoadSpriteByName(string path, string spriteName)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .FirstOrDefault(candidate => candidate.name == spriteName);
        if (sprite == null)
            throw new InvalidOperationException($"Missing sprite {spriteName} at {path}.");
        return sprite;
    }

    private static Sprite LoadSpriteAtPath(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            throw new InvalidOperationException($"Missing sprite at {path}.");
        return sprite;
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new InvalidOperationException($"Missing serialized property {propertyName} on {serializedObject.targetObject}.");
        property.objectReferenceValue = value;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void PlaceTop(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void PlaceCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void EnsureFolder(string path)
    {
        string[] segments = path.Split('/');
        string current = segments[0];
        for (int index = 1; index < segments.Length; index++)
        {
            string next = current + "/" + segments[index];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, segments[index]);
            current = next;
        }
    }
}
#endif
