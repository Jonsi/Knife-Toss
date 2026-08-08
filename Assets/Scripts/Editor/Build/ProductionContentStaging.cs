using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Build
{
    /// <summary>
    /// Keeps development-only Resources available to Development/QA profiles while moving
    /// them outside Unity's import graph for Production. Directory GUIDs are restored when
    /// a non-production profile becomes active, so scene and prefab references remain stable.
    /// </summary>
    [InitializeOnLoad]
    internal static class ProductionContentStaging
    {
        private const string DisabledSuffix = "_DISABLED~";

        private static readonly string[] DevelopmentOnlyDirectories =
        {
            "Assets/Resources/Debug",
            "Assets/Scripts/Common/Ads/Mock",
            "Assets/Scripts/Common/Analytics/Mock",
            "Assets/Scripts/Common/IAP/Mock",
            "Assets/Scripts/Game/Ads/Mock",
            "Assets/Scripts/Game/IAP/Mock",
            "Assets/StompyRobot/SRDebugger/Resources",
            "Assets/StompyRobot/SRDebugger/usr",
            "Assets/StompyRobot/SRDebugger/UI/Prefabs"
        };

        private static bool? _AppliedProductionState;

        static ProductionContentStaging()
        {
            EditorApplication.update += MonitorActiveProfile;
            EditorApplication.delayCall += SynchronizeWithActiveProfile;
        }

        public static IReadOnlyList<string> CollectProductionViolations()
        {
            return DevelopmentOnlyDirectories
                .Where(Directory.Exists)
                .Select(path => $"Development-only content is still enabled at {path}.")
                .ToArray();
        }

        [MenuItem("Knife Toss/Build/Synchronize Development-Only Content")]
        private static void SynchronizeWithActiveProfile()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            bool production = EditorUserBuildSettings.activeScriptCompilationDefines
                .Contains(ProductionBuildPolicy.ProductionDefine, StringComparer.Ordinal);
            bool changed = false;

            AssetDatabase.ReleaseCachedFileHandles();
            foreach (string enabledPath in DevelopmentOnlyDirectories)
            {
                changed |= SetDirectoryEnabled(enabledPath, !production);
            }

            if (!changed)
            {
                _AppliedProductionState = production;
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            _AppliedProductionState = production;
            Debug.Log(production
                ? "[BuildPolicy] Production content staged; debug Resources are excluded."
                : "[BuildPolicy] Development-only Resources restored.");
        }

        private static void MonitorActiveProfile()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            bool production = EditorUserBuildSettings.activeScriptCompilationDefines
                .Contains(ProductionBuildPolicy.ProductionDefine, StringComparer.Ordinal);
            if (_AppliedProductionState == production)
            {
                return;
            }

            SynchronizeWithActiveProfile();
        }

        private static bool SetDirectoryEnabled(string enabledPath, bool enable)
        {
            string disabledPath = enabledPath + DisabledSuffix;
            string enabledMetaPath = enabledPath + ".meta";
            string disabledMetaPath = disabledPath + ".meta";
            string disabledMetaBackupPath = disabledPath + ".meta.bak~";
            bool isEnabled = Directory.Exists(enabledPath);
            bool isDisabled = Directory.Exists(disabledPath);

            if (isEnabled && isDisabled)
            {
                throw new InvalidOperationException(
                    $"Both enabled and disabled content directories exist: {enabledPath}");
            }

            if ((enable && isEnabled) || (!enable && isDisabled))
            {
                return false;
            }

            string source = enable ? disabledPath : enabledPath;
            string destination = enable ? enabledPath : disabledPath;
            if (!Directory.Exists(source))
            {
                return false;
            }

            Directory.Move(source, destination);

            if (!enable)
            {
                if (File.Exists(disabledMetaBackupPath))
                {
                    File.Delete(disabledMetaBackupPath);
                }

                if (File.Exists(disabledMetaPath))
                {
                    File.Move(disabledMetaPath, disabledMetaBackupPath);
                }
                else if (File.Exists(enabledMetaPath))
                {
                    File.Move(enabledMetaPath, disabledMetaBackupPath);
                }
            }
            else if (File.Exists(disabledMetaBackupPath))
            {
                if (File.Exists(enabledMetaPath))
                {
                    File.Delete(enabledMetaPath);
                }

                File.Move(disabledMetaBackupPath, enabledMetaPath);
            }

            return true;
        }
    }
}
