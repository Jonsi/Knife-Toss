using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor.Build
{
    /// <summary>
    /// Prevents development and QA tooling from being compiled into a non-development
    /// player. Production builds must opt in with PRODUCTION_BUILD and must not contain
    /// TEST_ENABLED or QA_BUILD.
    /// </summary>
    public sealed class ProductionBuildPolicy : IPreprocessBuildWithReport
    {
        public const string TestDefine = "TEST_ENABLED";
        public const string QaDefine = "QA_BUILD";
        public const string ProductionDefine = "PRODUCTION_BUILD";
        public const string DisableSrDebuggerDefine = "DISABLE_SRDEBUGGER";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            bool isDevelopment = (report.summary.options & BuildOptions.Development) != 0;
            string[] defines = EditorUserBuildSettings.activeScriptCompilationDefines ?? Array.Empty<string>();
            var violations = new List<string>(CollectViolations(isDevelopment, defines));
            if (defines.Contains(ProductionDefine, StringComparer.Ordinal))
            {
                violations.AddRange(ProductionContentStaging.CollectProductionViolations());
            }

            if (violations.Count == 0)
            {
                return;
            }

            throw new BuildFailedException(
                "Build policy validation failed:\n- " + string.Join("\n- ", violations));
        }

        public static IReadOnlyList<string> CollectViolations(
            bool isDevelopment,
            IEnumerable<string> activeDefines)
        {
            var defines = new HashSet<string>(
                activeDefines?.Where(define => !string.IsNullOrWhiteSpace(define)) ??
                Enumerable.Empty<string>(),
                StringComparer.Ordinal);
            var violations = new List<string>();
            bool isProductionProfile = defines.Contains(ProductionDefine);
            bool isQaProfile = defines.Contains(QaDefine);
            bool hasTestTools = defines.Contains(TestDefine);
            bool disablesSrDebugger = defines.Contains(DisableSrDebuggerDefine);

            if (!isDevelopment && !isProductionProfile)
            {
                violations.Add(
                    $"Non-development builds must use a profile containing {ProductionDefine}.");
            }

            if (!isDevelopment && hasTestTools)
            {
                violations.Add($"Non-development builds cannot contain {TestDefine}.");
            }

            if (isProductionProfile && isDevelopment)
            {
                violations.Add("The production profile cannot use Development Build.");
            }

            if (isProductionProfile && hasTestTools)
            {
                violations.Add($"The production profile cannot contain {TestDefine}.");
            }

            if (isProductionProfile && isQaProfile)
            {
                violations.Add($"The production profile cannot contain {QaDefine}.");
            }

            if (isProductionProfile && !disablesSrDebugger)
            {
                violations.Add(
                    $"The production profile must contain {DisableSrDebuggerDefine}.");
            }

            if (isQaProfile && !isDevelopment)
            {
                violations.Add("The QA profile must use Development Build.");
            }

            if (isQaProfile && !hasTestTools)
            {
                violations.Add($"The QA profile must contain {TestDefine}.");
            }

            return violations;
        }

        [MenuItem("Knife Toss/Build/Validate Active Build Policy")]
        private static void ValidateActiveConfiguration()
        {
            IReadOnlyList<string> violations = CollectViolations(
                EditorUserBuildSettings.development,
                EditorUserBuildSettings.activeScriptCompilationDefines);

            if (violations.Count > 0)
            {
                Debug.LogError("[BuildPolicy] Invalid active configuration:\n- " +
                               string.Join("\n- ", violations));
                return;
            }

            Debug.Log("[BuildPolicy] Active build configuration is valid.");
        }
    }
}
