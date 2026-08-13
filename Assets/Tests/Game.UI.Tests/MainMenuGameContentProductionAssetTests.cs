using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI.Tests
{
    public sealed class MainMenuGameContentProductionAssetTests
    {
        private const string KnifePath = "Assets/Art/gameplay/knife.png";
        private const string TargetPath = "Assets/Art/knife_pick_target.png";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [Test]
        public void DefaultKnife_PreservesGameplayWorldSizeAtProductionDensity()
        {
            var importer = AssetImporter.GetAtPath(KnifePath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Multiple));
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(300f));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(KnifePath);
            Sprite knife = AssetDatabase.LoadAllAssetsAtPath(KnifePath).OfType<Sprite>().Single();
            Assert.That(texture.width, Is.EqualTo(256));
            Assert.That(texture.height, Is.EqualTo(384));
            Assert.That(knife.name, Is.EqualTo("Knife_0"));
            Assert.That(knife.rect.size, Is.EqualTo(new Vector2(108f, 375f)));
            Assert.That(knife.bounds.size.x, Is.EqualTo(0.36f).Within(0.001f));
            Assert.That(knife.bounds.size.y, Is.EqualTo(1.25f).Within(0.001f));

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_4x4));
            Assert.That(android.crunchedCompression, Is.False);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Knife.prefab");
            Assert.That(prefab.GetComponent<SpriteRenderer>().sprite, Is.EqualTo(knife));
        }

        [Test]
        public void MainMenuTarget_UsesTheProductionTextureWithoutChangingSceneOwnership()
        {
            var importer = AssetImporter.GetAtPath(TargetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(200f));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetPath);
            Assert.That(texture.width, Is.EqualTo(1536));
            Assert.That(texture.height, Is.EqualTo(1024));

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(2048));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_4x4));
            Assert.That(android.crunchedCompression, Is.False);

            Scene scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                Transform target = Find(scene, "Canvas/Layer1/SafeAreaRoot/KnifeSkinSlelector/bg");
                Image targetImage = target.GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(targetImage.sprite), Is.EqualTo(TargetPath));
                Assert.That(targetImage.raycastTarget, Is.False);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static Transform Find(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = scene.GetRootGameObjects().Single(root => root.name == parts[0]).transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }
    }
}
