using System;
using System.Collections.Generic;
using System.IO;
using Game.Installers;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Installers
{
    public static class InstallerAssetCreator
    {
        private const string InstallersFolder = "Assets/Configurations/Installers";

        private static readonly (Type type, string assetName)[] InstallerAssets =
        {
            (typeof(UIInstallerSO), "UIInstaller.asset"),
            (typeof(AudioInstallerSO), "AudioInstaller.asset"),
            (typeof(KnifeInstallerSO), "KnifeInstaller.asset"),
            (typeof(LevelInstallerSO), "LevelInstaller.asset"),
            (typeof(StageInstallerSO), "StageInstaller.asset"),
            (typeof(TargetInstallerSO), "TargetInstaller.asset"),
            (typeof(CollisionInstallerSO), "CollisionInstaller.asset"),
            (typeof(PowerUpInstallerSO), "PowerUpInstaller.asset"),
            (typeof(VFXInstallerSO), "VFXInstaller.asset"),
            (typeof(PersistenceInstallerSO), "PersistenceInstaller.asset"),
            (typeof(GameFlowInstallerSO), "GameFlowInstaller.asset"),
            (typeof(CollectibleInstallerSO), "CollectibleInstaller.asset")
        };

        [MenuItem("Tools/Installers/Create All Installer Assets")]
        private static void CreateAllInstallerAssets()
        {
            EnsureFolderExists(InstallersFolder);

            var created = new List<string>();
            var skipped = new List<string>();

            foreach (var (type, assetName) in InstallerAssets)
            {
                var assetPath = $"{InstallersFolder}/{assetName}";
                var existing = AssetDatabase.LoadAssetAtPath(assetPath, type);
                if (existing != null)
                {
                    skipped.Add(assetName);
                    continue;
                }

                var instance = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, assetPath);
                created.Add(assetName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Installer assets created: {created.Count}, skipped: {skipped.Count}");
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
