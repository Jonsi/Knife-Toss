using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.UI.Tests
{
    public sealed class ButtonProductionAssetTests
    {
        private const string CoreUiAtlasPath = "Assets/Art/UI/Core/CoreUI.spriteatlasv2";
        private const string OwnershipMarksPath = "Assets/Art/UI/Core/ButtonOwnershipMarks.png";
        private static readonly string[] SharedActionSilhouettePaths =
        {
            "Assets/Art/UI/Core/ButtonPrimary.png",
            "Assets/Art/UI/Core/ButtonSecondary.png",
            "Assets/Art/UI/Core/ButtonDanger.png"
        };

        private static readonly TextureSpec[] TextureSpecs =
        {
            new TextureSpec(
                "Assets/Art/UI/Core/ButtonPrimary.png",
                768,
                204,
                SpriteImportMode.Single,
                new Vector4(48f, 30f, 48f, 30f),
                1024),
            new TextureSpec(
                "Assets/Art/UI/Core/ButtonSecondary.png",
                768,
                204,
                SpriteImportMode.Single,
                new Vector4(48f, 30f, 48f, 30f),
                1024),
            new TextureSpec(
                "Assets/Art/UI/Core/ButtonDanger.png",
                768,
                204,
                SpriteImportMode.Single,
                new Vector4(48f, 30f, 48f, 30f),
                1024),
            new TextureSpec(
                "Assets/Art/UI/Core/ButtonIconSocket.png",
                256,
                256,
                SpriteImportMode.Single,
                new Vector4(40f, 40f, 40f, 40f),
                256),
            new TextureSpec(
                "Assets/Art/UI/Core/ButtonPrice.png",
                768,
                200,
                SpriteImportMode.Single,
                new Vector4(48f, 28f, 48f, 28f),
                1024),
            new TextureSpec(
                "Assets/Art/UI/Core/ButtonStatus.png",
                768,
                172,
                SpriteImportMode.Single,
                new Vector4(42f, 24f, 42f, 24f),
                1024),
            new TextureSpec(
                OwnershipMarksPath,
                512,
                256,
                SpriteImportMode.Multiple,
                Vector4.zero,
                512)
        };

        private static readonly OwnershipSpriteSpec[] OwnershipSpriteSpecs =
        {
            new OwnershipSpriteSpec("ButtonOwnedMark", new Rect(0f, 0f, 256f, 256f)),
            new OwnershipSpriteSpec("ButtonEquippedMark", new Rect(256f, 0f, 256f, 256f))
        };

        private static readonly AtlasSpriteSpec[] AtlasSpriteSpecs =
        {
            new AtlasSpriteSpec("Assets/Art/UI/Core/ButtonPrimary.png", "ButtonPrimary"),
            new AtlasSpriteSpec("Assets/Art/UI/Core/ButtonSecondary.png", "ButtonSecondary"),
            new AtlasSpriteSpec("Assets/Art/UI/Core/ButtonDanger.png", "ButtonDanger"),
            new AtlasSpriteSpec("Assets/Art/UI/Core/ButtonIconSocket.png", "ButtonIconSocket"),
            new AtlasSpriteSpec("Assets/Art/UI/Core/ButtonPrice.png", "ButtonPrice"),
            new AtlasSpriteSpec("Assets/Art/UI/Core/ButtonStatus.png", "ButtonStatus"),
            new AtlasSpriteSpec(OwnershipMarksPath, "ButtonOwnedMark"),
            new AtlasSpriteSpec(OwnershipMarksPath, "ButtonEquippedMark")
        };

        public static IEnumerable<TestCaseData> ButtonTextureCases()
        {
            foreach (TextureSpec spec in TextureSpecs)
            {
                yield return new TestCaseData(spec).SetName($"ButtonTexture_{spec.AssetName}_MatchesProductionImportContract");
            }
        }

        [TestCaseSource(nameof(ButtonTextureCases))]
        public void ButtonTexturesMatchProductionImportContract(TextureSpec spec)
        {
            var importer = AssetImporter.GetAtPath(spec.Path) as TextureImporter;
            Assert.That(importer, Is.Not.Null, $"Missing TextureImporter for {spec.Path}.");

            importer.GetSourceTextureWidthAndHeight(out int sourceWidth, out int sourceHeight);
            Assert.That(sourceWidth, Is.EqualTo(spec.Width), $"Unexpected source width for {spec.Path}.");
            Assert.That(sourceHeight, Is.EqualTo(spec.Height), $"Unexpected source height for {spec.Path}.");
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), spec.Path);
            Assert.That(importer.spriteImportMode, Is.EqualTo(spec.ImportMode), spec.Path);
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(100f), spec.Path);
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            Assert.That(textureSettings.spriteMeshType, Is.EqualTo(SpriteMeshType.FullRect), spec.Path);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear), spec.Path);
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp), spec.Path);
            Assert.That(importer.sRGBTexture, Is.True, spec.Path);
            Assert.That(importer.mipmapEnabled, Is.False, spec.Path);
            Assert.That(importer.isReadable, Is.False, spec.Path);
            Assert.That(importer.DoesSourceTextureHaveAlpha(), Is.True, spec.Path);
            Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromInput), spec.Path);
            Assert.That(importer.alphaIsTransparency, Is.True, spec.Path);
            Assert.That(importer.spriteBorder, Is.EqualTo(spec.Border), spec.Path);
            Assert.That(importer.maxTextureSize, Is.EqualTo(spec.AndroidMaxSize), spec.Path);

            TextureImporterPlatformSettings defaultPlatform =
                importer.GetPlatformTextureSettings("DefaultTexturePlatform");
            Assert.That(defaultPlatform.maxTextureSize, Is.EqualTo(spec.AndroidMaxSize), spec.Path);

            // Legacy packing tags are obsolete, but must remain empty when V2 atlas membership is authoritative.
#pragma warning disable CS0618
            Assert.That(importer.spritePackingTag, Is.Empty, spec.Path);
#pragma warning restore CS0618

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            AssertAndroidSettings(android, spec.AndroidMaxSize, spec.Path);
            AssertSourceAlphaContract(spec);
        }

        [Test]
        public void OwnershipSheetContainsOnlyTheExactOwnedAndEquippedSprites()
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(OwnershipMarksPath)
                .OfType<Sprite>()
                .ToArray();

            Assert.That(sprites, Has.Length.EqualTo(OwnershipSpriteSpecs.Length));
            CollectionAssert.AreEquivalent(
                OwnershipSpriteSpecs.Select(spec => spec.Name),
                sprites.Select(sprite => sprite.name));

            foreach (OwnershipSpriteSpec spec in OwnershipSpriteSpecs)
            {
                Sprite sprite = sprites.Single(candidate => candidate.name == spec.Name);
                Assert.That(sprite.rect, Is.EqualTo(spec.Rect), $"Unexpected rect for {spec.Name}.");
                Assert.That(sprite.border, Is.EqualTo(Vector4.zero), $"Unexpected border for {spec.Name}.");
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(100f), $"Unexpected PPU for {spec.Name}.");
            }
        }

        [Test]
        public void PrimarySecondaryAndDangerShareAnExactPixelAlphaSilhouette()
        {
            byte[][] alphaChannels = SharedActionSilhouettePaths
                .Select(LoadSourceAlphaChannel)
                .ToArray();

            Assert.That(alphaChannels, Has.Length.EqualTo(3));
            CollectionAssert.AreEqual(alphaChannels[0], alphaChannels[1],
                "Secondary must preserve the Primary action-button silhouette pixel-for-pixel.");
            CollectionAssert.AreEqual(alphaChannels[0], alphaChannels[2],
                "Danger must preserve the Primary action-button silhouette pixel-for-pixel.");
        }

        [Test]
        public void CoreUiIsAValidV2AtlasWithProductionImporterSettings()
        {
            SpriteAtlasAsset atlasAsset = SpriteAtlasAsset.Load(CoreUiAtlasPath);
            Assert.That(atlasAsset, Is.Not.Null, $"Missing V2 SpriteAtlasAsset at {CoreUiAtlasPath}.");
            Assert.That(atlasAsset.isVariant, Is.False);
            Assert.That(atlasAsset.GetMasterAtlas(), Is.Null);

            var importer = AssetImporter.GetAtPath(CoreUiAtlasPath) as SpriteAtlasImporter;
            Assert.That(importer, Is.Not.Null, $"Missing SpriteAtlasImporter for {CoreUiAtlasPath}.");
            Assert.That(importer.includeInBuild, Is.True);
            Assert.That(importer.variantScale, Is.EqualTo(1f));
            AssertPackingSettings(importer.packingSettings, "SpriteAtlasImporter");
            AssertTextureSettings(importer.textureSettings, "SpriteAtlasImporter");
            AssertAndroidSettings(importer.GetPlatformSettings("Android"), 2048, "SpriteAtlasImporter");

            SpriteAtlas runtimeAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(CoreUiAtlasPath);
            Assert.That(runtimeAtlas, Is.Not.Null, "The V2 atlas importer did not generate a SpriteAtlas.");
            Assert.That(runtimeAtlas.name, Is.EqualTo("CoreUI"));
            Assert.That(runtimeAtlas.tag, Is.EqualTo("CoreUI"));
            Assert.That(runtimeAtlas.isVariant, Is.False);
            Assert.That(runtimeAtlas.spriteCount, Is.EqualTo(AtlasSpriteSpecs.Length));
        }

        [Test]
        public void CoreUiAtlasContainsExactlyTheEightRequiredSpriteReferences()
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(CoreUiAtlasPath);
            Assert.That(atlas, Is.Not.Null, $"Missing generated SpriteAtlas at {CoreUiAtlasPath}.");

            UnityEngine.Object[] packables = SpriteAtlasExtensions.GetPackables(atlas);
            Assert.That(packables, Is.Not.Null);
            Assert.That(packables, Has.Length.EqualTo(AtlasSpriteSpecs.Length));

            foreach (UnityEngine.Object packable in packables)
            {
                Assert.That(packable, Is.Not.Null, "CoreUI contains a missing packable reference.");
                Assert.That(packable, Is.InstanceOf<Sprite>(), "CoreUI packables must be exact Sprite references.");
            }

            Sprite[] expectedSprites = AtlasSpriteSpecs.Select(LoadExpectedSprite).ToArray();
            string[] expectedIds = expectedSprites.Select(GetStableAssetId).ToArray();
            string[] actualIds = packables.Select(GetStableAssetId).ToArray();

            Assert.That(new HashSet<string>(actualIds).Count, Is.EqualTo(actualIds.Length),
                "CoreUI contains duplicate packable references.");
            CollectionAssert.AreEquivalent(expectedIds, actualIds,
                "CoreUI packables differ from the exact eight production button sprites.");

            foreach (Sprite expectedSprite in expectedSprites)
            {
                Assert.That(atlas.CanBindTo(expectedSprite), Is.True,
                    $"CoreUI cannot bind required sprite {expectedSprite.name}.");
            }
        }

        private static void AssertPackingSettings(SpriteAtlasPackingSettings settings, string owner)
        {
            Assert.That(settings.padding, Is.EqualTo(8), owner);
            Assert.That(settings.blockOffset, Is.EqualTo(1), owner);
            Assert.That(settings.enableRotation, Is.False, owner);
            Assert.That(settings.enableTightPacking, Is.False, owner);
            Assert.That(settings.enableAlphaDilation, Is.False, owner);
        }

        private static void AssertTextureSettings(SpriteAtlasTextureSettings settings, string owner)
        {
            Assert.That(settings.maxTextureSize, Is.EqualTo(2048), owner);
            Assert.That(settings.anisoLevel, Is.EqualTo(1), owner);
            Assert.That(settings.filterMode, Is.EqualTo(FilterMode.Bilinear), owner);
            Assert.That(settings.generateMipMaps, Is.False, owner);
            Assert.That(settings.readable, Is.False, owner);
            Assert.That(settings.sRGB, Is.True, owner);
        }

        private static void AssertAndroidSettings(
            TextureImporterPlatformSettings settings,
            int expectedMaxSize,
            string owner)
        {
            Assert.That(settings.overridden, Is.True, owner);
            Assert.That(settings.maxTextureSize, Is.EqualTo(expectedMaxSize), owner);
            Assert.That(settings.format, Is.EqualTo(TextureImporterFormat.ASTC_4x4), owner);
            Assert.That(settings.crunchedCompression, Is.False, owner);
            Assert.That(settings.allowsAlphaSplitting, Is.False, owner);
        }

        private static void AssertSourceAlphaContract(TextureSpec spec)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string absolutePath = Path.Combine(projectRoot, spec.Path);
            Assert.That(File.Exists(absolutePath), Is.True, $"Missing source image at {spec.Path}.");

            Texture2D sourceTexture = null;
            try
            {
                sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
                bool loaded = ImageConversion.LoadImage(
                    sourceTexture,
                    File.ReadAllBytes(absolutePath),
                    false);
                Assert.That(loaded, Is.True, $"Could not decode source image {spec.Path}.");
                Assert.That(sourceTexture.width, Is.EqualTo(spec.Width), spec.Path);
                Assert.That(sourceTexture.height, Is.EqualTo(spec.Height), spec.Path);

                Color32[] pixels = sourceTexture.GetPixels32();
                Assert.That(pixels.Any(pixel => pixel.a == byte.MinValue), Is.True,
                    $"{spec.Path} has no fully transparent source pixels.");
                Assert.That(pixels.Any(pixel => pixel.a == byte.MaxValue), Is.True,
                    $"{spec.Path} has no fully opaque source pixels.");

                if (spec.ImportMode == SpriteImportMode.Multiple)
                {
                    return;
                }

                int xMin = (int)spec.Border.x;
                int yMin = (int)spec.Border.y;
                int xMax = spec.Width - (int)spec.Border.z;
                int yMax = spec.Height - (int)spec.Border.w;
                Assert.That(xMax, Is.GreaterThan(xMin), $"Invalid horizontal center region for {spec.Path}.");
                Assert.That(yMax, Is.GreaterThan(yMin), $"Invalid vertical center region for {spec.Path}.");

                int nonOpaqueX = -1;
                int nonOpaqueY = -1;
                byte nonOpaqueAlpha = byte.MaxValue;
                for (int y = yMin; y < yMax && nonOpaqueX < 0; y++)
                {
                    for (int x = xMin; x < xMax; x++)
                    {
                        byte alpha = pixels[(y * spec.Width) + x].a;
                        if (alpha == byte.MaxValue)
                        {
                            continue;
                        }

                        nonOpaqueX = x;
                        nonOpaqueY = y;
                        nonOpaqueAlpha = alpha;
                        break;
                    }
                }

                Assert.That(nonOpaqueX, Is.LessThan(0),
                    $"{spec.Path} center pixel ({nonOpaqueX}, {nonOpaqueY}) has alpha {nonOpaqueAlpha}, not 255.");
            }
            finally
            {
                if (sourceTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(sourceTexture);
                }
            }
        }

        private static Sprite LoadExpectedSprite(AtlasSpriteSpec spec)
        {
            Sprite[] matches = AssetDatabase.LoadAllAssetsAtPath(spec.Path)
                .OfType<Sprite>()
                .Where(sprite => sprite.name == spec.Name)
                .ToArray();

            Assert.That(matches, Has.Length.EqualTo(1),
                $"Expected one sprite named {spec.Name} at {spec.Path}.");
            return matches[0];
        }

        private static byte[] LoadSourceAlphaChannel(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string absolutePath = Path.Combine(projectRoot, assetPath);
            Assert.That(File.Exists(absolutePath), Is.True, $"Missing source image at {assetPath}.");

            Texture2D sourceTexture = null;
            try
            {
                sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
                Assert.That(ImageConversion.LoadImage(sourceTexture, File.ReadAllBytes(absolutePath), false), Is.True,
                    $"Could not decode source image {assetPath}.");
                return sourceTexture.GetPixels32().Select(pixel => pixel.a).ToArray();
            }
            finally
            {
                if (sourceTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(sourceTexture);
                }
            }
        }

        private static string GetStableAssetId(UnityEngine.Object asset)
        {
            Assert.That(asset, Is.Not.Null, "Cannot resolve an asset ID for a missing reference.");
            bool resolved = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                asset,
                out string guid,
                out long localId);
            Assert.That(resolved, Is.True, $"Could not resolve the asset ID for {asset.name}.");
            return $"{guid}:{localId}";
        }

        public sealed class TextureSpec
        {
            public TextureSpec(
                string path,
                int width,
                int height,
                SpriteImportMode importMode,
                Vector4 border,
                int androidMaxSize)
            {
                Path = path;
                Width = width;
                Height = height;
                ImportMode = importMode;
                Border = border;
                AndroidMaxSize = androidMaxSize;
            }

            public string Path { get; }
            public int Width { get; }
            public int Height { get; }
            public SpriteImportMode ImportMode { get; }
            public Vector4 Border { get; }
            public int AndroidMaxSize { get; }
            public string AssetName => System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        private sealed class OwnershipSpriteSpec
        {
            public OwnershipSpriteSpec(string name, Rect rect)
            {
                Name = name;
                Rect = rect;
            }

            public string Name { get; }
            public Rect Rect { get; }
        }

        private sealed class AtlasSpriteSpec
        {
            public AtlasSpriteSpec(string path, string name)
            {
                Path = path;
                Name = name;
            }

            public string Path { get; }
            public string Name { get; }
        }
    }
}
