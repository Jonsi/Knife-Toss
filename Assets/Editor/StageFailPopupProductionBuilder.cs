#if UNITY_EDITOR
using System;
using System.Linq;
using DG.Tweening;
using Game.UI.Common;
using Game.UI.Popups;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;

public static class StageFailPopupProductionBuilder
{
    private const string StageScenePath = "Assets/Scenes/StagePlay.unity";
    private const string ActionButtonPrefabPath = "Assets/Prefabs/UI/Buttons/FailureActionButton.prefab";
    private const string PopupTemplatePath = "Assets/Prefabs/UI/PopupTemplate.prefab";
    private const string PopupFramePath = "Assets/Art/popup_frame.png";
    private const string AtlasPath = "Assets/Art/UI/Failure/FailurePopupAtlas.png";
    private const string KnifeAnimationAtlasPath = "Assets/Art/UI/Failure/FailureKnivesAnimation.png";
    private const int KnifeAnimationFrameCount = 16;

    private const string TitleSpriteName = "GameOverTitle";
    private const string KnivesSpriteName = "CrossedKnives";
    private const string RetrySpriteName = "RetryButtonArt";
    private const string MenuSpriteName = "MenuButtonArt";

    [MenuItem("Tools/Toss Madness/UI/Build Approved Failure Popup")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Build the failure popup outside Play Mode.");

        EnsureFolder("Assets/Prefabs/UI/Buttons");
        bool importerChanged = ConfigureAtlasImporter();
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

    private static bool ConfigureAtlasImporter()
    {
        TextureImporter importer = AssetImporter.GetAtPath(AtlasPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Missing failure popup atlas at {AtlasPath}.");

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
        SpriteRect[] desired = new[]
        {
            SpriteData(TitleSpriteName, new Rect(10f, 899f, 530f, 115f), existing),
            SpriteData(KnivesSpriteName, new Rect(10f, 449f, 385f, 430f), existing),
            SpriteData(RetrySpriteName, new Rect(10f, 209f, 570f, 215f), existing),
            SpriteData(MenuSpriteName, new Rect(10f, 39f, 415f, 145f), existing),
        };

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
            rootRect.sizeDelta = new Vector2(565f, 230f);
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
            visual.sprite = LoadAtlasSprite(RetrySpriteName);
            visual.type = Image.Type.Simple;
            visual.preserveAspect = true;
            visual.color = Color.white;
            visual.raycastTarget = false;

            ActionButtonPresenter presenter = GetOrAdd<ActionButtonPresenter>(root);
            SerializedObject presenterObject = new SerializedObject(presenter);
            SetReference(presenterObject, "_Button", button);
            SetReference(presenterObject, "_PresentationRoot", visualRoot);
            SetReference(presenterObject, "_InteractionGroup", interactionGroup);
            SetReference(presenterObject, "_HitAreaGraphic", hitArea);
            SetReference(presenterObject, "_SurfaceImage", null);
            presenterObject.FindProperty("_SkinStyle").enumValueIndex = (int)Game.UI.Tokens.UiButtonStyle.Unskinned;
            presenterObject.ApplyModifiedPropertiesWithoutUndo();

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
        scrim.color = new Color(0f, 0f, 0f, 0.95f);
        scrim.raycastTarget = true;
        PrefabUtility.RecordPrefabInstancePropertyModifications(scrim);

        RectTransform holderRect = (RectTransform)holder;
        holderRect.anchorMin = new Vector2(0.1f, 0.1f);
        holderRect.anchorMax = new Vector2(0.9f, 0.9f);
        holderRect.offsetMin = Vector2.zero;
        holderRect.offsetMax = Vector2.zero;
        holderRect.localScale = Vector3.one;
        PrefabUtility.RecordPrefabInstancePropertyModifications(holderRect);

        Image holderImage = holder.GetComponent<Image>();
        holderImage.sprite = LoadSpriteByName(PopupFramePath, "popup_frame_0");
        holderImage.type = Image.Type.Sliced;
        holderImage.preserveAspect = false;
        holderImage.color = Color.white;
        holderImage.raycastTarget = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(holderImage);

        RectTransform contentRect = (RectTransform)content;
        Stretch(contentRect, Vector2.zero, Vector2.zero);
        PrefabUtility.RecordPrefabInstancePropertyModifications(contentRect);
        ClearChildren(content);

        Image title = CreateImage("GameOverHeading", content, LoadAtlasSprite(TitleSpriteName));
        PlaceTop(title.rectTransform, new Vector2(0f, -175f), new Vector2(600f, 146f));
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

        Sprite[] knifeFrames = LoadKnifeAnimationFrames();
        Image knives = CreateImage("CrossedKnives", content, knifeFrames[0]);
        PlaceTop(knives.rectTransform, new Vector2(0f, -325f), new Vector2(485f, 485f));
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
            false);

        GameObject buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ActionButtonPrefabPath);
        if (buttonPrefab == null)
            throw new InvalidOperationException($"Missing shared action button prefab: {ActionButtonPrefabPath}");

        GameObject retryObject = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, content);
        retryObject.name = "RetryButton";
        RectTransform retryRect = retryObject.GetComponent<RectTransform>();
        PlaceTop(retryRect, new Vector2(0f, -925f), new Vector2(650f, 230f));
        ConfigureButtonInstance(retryObject, RetrySpriteName);
        ConfigureElementAnimator(
            retryObject,
            retryRect,
            retryObject.transform.Find("VisualRoot").GetComponent<Image>(),
            new Vector2(0f, -24f),
            new Vector3(0.9f, 0.9f, 1f),
            Vector3.zero,
            0.3f,
            0.34f,
            true);

        GameObject menuObject = (GameObject)PrefabUtility.InstantiatePrefab(buttonPrefab, content);
        menuObject.name = "MenuButton";
        RectTransform menuRect = menuObject.GetComponent<RectTransform>();
        PlaceTop(menuRect, new Vector2(0f, -1165f), new Vector2(455f, 150f));
        ConfigureButtonInstance(menuObject, MenuSpriteName);
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
        SetReference(popupObject, "_RetryButton", retryObject.GetComponent<Button>());
        SetReference(popupObject, "_MenuButton", menuObject.GetComponent<Button>());
        popupObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(popup);
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

    private static void ConfigureButtonInstance(GameObject buttonObject, string spriteName)
    {
        Image visual = buttonObject.transform.Find("VisualRoot")?.GetComponent<Image>();
        if (visual == null)
            throw new InvalidOperationException($"{buttonObject.name} lost its shared VisualRoot image.");

        visual.sprite = LoadAtlasSprite(spriteName);
        visual.preserveAspect = true;
        visual.color = Color.white;
        PrefabUtility.RecordPrefabInstancePropertyModifications(visual);
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

        if (popup.GetComponentsInChildren<TMPro.TMP_Text>(true).Length != 0)
            return "FAIL: reference-derived popup must not contain substitute runtime typography.";

        string[] requiredSprites = { TitleSpriteName, RetrySpriteName, MenuSpriteName };
        string[] actualSprites = popup.GetComponentsInChildren<Image>(true)
            .Where(image => image.sprite != null && AssetDatabase.GetAssetPath(image.sprite) == AtlasPath)
            .Select(image => image.sprite.name)
            .Distinct()
            .ToArray();
        if (requiredSprites.Any(required => !actualSprites.Contains(required)))
            return $"FAIL: popup atlas sprites were [{string.Join(", ", actualSprites)}].";

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
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(button.gameObject);
            if (source == null || AssetDatabase.GetAssetPath(source) != ActionButtonPrefabPath)
                return $"FAIL: {button.name} is not linked to {ActionButtonPrefabPath}.";
        }

        GameObject popupSource = PrefabUtility.GetCorrespondingObjectFromSource(popup.gameObject);
        if (popupSource == null || AssetDatabase.GetAssetPath(popupSource) != PopupTemplatePath)
            return $"FAIL: StageFailPopup is not linked to {PopupTemplatePath}.";

        SerializedObject popupObject = new SerializedObject(popup);
        if (popupObject.FindProperty("_RetryButton")?.objectReferenceValue == null ||
            popupObject.FindProperty("_MenuButton")?.objectReferenceValue == null)
            return "FAIL: StageFailPopup button bindings are incomplete.";

        return "PASS: approved Failure popup uses the existing frame, direct-reference lettering/actions, a sixteen-frame independent-knife impact, two shared-prefab actions, valid bindings, and PopupTemplate linkage.";
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

    private static Sprite LoadAtlasSprite(string spriteName)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(AtlasPath)
            .OfType<Sprite>()
            .FirstOrDefault(candidate => candidate.name == spriteName);
        if (sprite == null)
            throw new InvalidOperationException($"Missing sprite {spriteName} in {AtlasPath}.");
        return sprite;
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
