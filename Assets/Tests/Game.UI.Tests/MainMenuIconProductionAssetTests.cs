using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.UI.Tests
{
    public sealed class MainMenuIconProductionAssetTests
    {
        private const string IconFolder = "Assets/Art/UI/Icons";
        private const string AtlasPath = IconFolder + "/MainMenuIcons.spriteatlasv2";

        private static readonly string[] IconNames =
        {
            "IconSettings",
            "IconStore",
            "IconMissions",
            "IconRemoveAds",
            "IconPrevious",
            "IconNext",
            "IconCurrency"
        };

        [Test]
        public void IllustratedIcons_UseTheProductionSpriteImportContract()
        {
            foreach (string name in IconNames)
            {
                string path = $"{IconFolder}/{name}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, $"Missing TextureImporter for {path}.");
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
                Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
                Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(100f));
                Assert.That(importer.alphaIsTransparency, Is.True);
                Assert.That(importer.mipmapEnabled, Is.False);
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
                Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
                Assert.That(importer.maxTextureSize, Is.EqualTo(256));

                TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
                Assert.That(android.overridden, Is.True);
                Assert.That(android.maxTextureSize, Is.EqualTo(256));
                Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_4x4));
                Assert.That(android.crunchedCompression, Is.False);

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.That(sprite, Is.Not.Null);
                Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(256f, 256f)));
                Assert.That(sprite.border, Is.EqualTo(Vector4.zero));
            }
        }

        [Test]
        public void MainMenuIconAtlas_ContainsExactlyTheSevenIllustratedSprites()
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            Assert.That(atlas, Is.Not.Null);
            Assert.That(atlas.spriteCount, Is.EqualTo(IconNames.Length));

            SpriteAtlasPackingSettings packing = atlas.GetPackingSettings();
            Assert.That(packing.padding, Is.GreaterThanOrEqualTo(8));
            Assert.That(packing.enableRotation, Is.False);
            Assert.That(packing.enableTightPacking, Is.False);
            Assert.That(packing.enableAlphaDilation, Is.True);

            Object[] packables = SpriteAtlasExtensions.GetPackables(atlas);
            Assert.That(packables, Has.Length.EqualTo(IconNames.Length));
            CollectionAssert.AreEquivalent(IconNames, packables.Select(packable => packable.name));
            Assert.That(new HashSet<Object>(packables).Count, Is.EqualTo(packables.Length));

            foreach (string name in IconNames)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconFolder}/{name}.png");
                Assert.That(atlas.CanBindTo(sprite), Is.True, $"Atlas cannot bind {name}.");
            }
        }
    }
}
