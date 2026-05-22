using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Editor.Build
{
    /// <summary>
    /// Enables iOS Simulator builds by stripping device-only Firebase native libraries and defining DISABLE_FIREBASE.
    /// </summary>
    public static class IosSimulatorFirebaseBuildProcessor
    {
        private const string DisableFirebaseDefine = "DISABLE_FIREBASE";

        private static readonly string[] FirebaseNativeLibraryPaths =
        {
            "Libraries/Plugins/iOS/Firebase/libFirebaseCppApp.a",
            "Libraries/Plugins/iOS/Firebase/libFirebaseCppRemoteConfig.a"
        };

        /// <summary>
        /// Adds or removes DISABLE_FIREBASE based on the active iOS SDK before the player build runs.
        /// </summary>
        public class Preprocessor : IPreprocessBuildWithReport
        {
            public int callbackOrder => 0;

            public void OnPreprocessBuild(BuildReport report)
            {
                if (report.summary.platform != BuildTarget.iOS)
                    return;

                UpdateScriptingDefines(PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK);
            }
        }

        /// <summary>
        /// Removes device-only Firebase static libraries from the generated Xcode project for simulator builds.
        /// </summary>
        public class Postprocessor : IPostprocessBuildWithReport
        {
            public int callbackOrder => 999;

            public void OnPostprocessBuild(BuildReport report)
            {
                if (report.summary.platform != BuildTarget.iOS)
                    return;

                if (PlayerSettings.iOS.sdkVersion != iOSSdkVersion.SimulatorSDK)
                    return;

                var buildPath = report.summary.outputPath;
                RemoveFirebaseNativeLibrariesFromXcodeProject(buildPath);
                Debug.Log("[iOS Simulator] Firebase native libraries removed. Remote Config uses local defaults (DISABLE_FIREBASE).");
            }
        }

        private static void UpdateScriptingDefines(bool disableFirebase)
        {
            var target = NamedBuildTarget.iOS;
            var defines = PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(';')
                .Where(define => !string.IsNullOrWhiteSpace(define))
                .ToList();

            if (disableFirebase)
            {
                if (!defines.Contains(DisableFirebaseDefine))
                    defines.Add(DisableFirebaseDefine);
            }
            else
            {
                defines.Remove(DisableFirebaseDefine);
            }

            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
        }

        private static void RemoveFirebaseNativeLibrariesFromXcodeProject(string buildPath)
        {
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            var targetGuids = new[]
            {
                project.GetUnityMainTargetGuid(),
                project.GetUnityFrameworkTargetGuid()
            };

            foreach (var libraryPath in FirebaseNativeLibraryPaths)
            {
                var fileGuid = project.FindFileGuidByProjectPath(libraryPath);
                if (string.IsNullOrEmpty(fileGuid))
                    continue;

                foreach (var targetGuid in targetGuids)
                    project.RemoveFileFromBuild(targetGuid, fileGuid);

                project.RemoveFile(fileGuid);

                var absolutePath = Path.Combine(buildPath, libraryPath);
                if (File.Exists(absolutePath))
                    File.Delete(absolutePath);
            }

            project.WriteToFile(projectPath);
        }
    }
}
