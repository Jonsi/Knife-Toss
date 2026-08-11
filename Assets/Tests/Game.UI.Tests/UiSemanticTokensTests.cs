using System;
using System.Collections.Generic;
using Game.UI.Common;
using Game.UI.Tokens;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.UI.Tests
{
    public sealed class UiSemanticTokensTests
    {
        [Test]
        public void ProductionAssetIsCompleteAndUsesCurrentSchema()
        {
            UiSemanticTokens tokens = AssetDatabase.LoadAssetAtPath<UiSemanticTokens>(
                UiSemanticTokens.ProductionAssetPath);

            Assert.That(tokens, Is.Not.Null, $"Missing production tokens at {UiSemanticTokens.ProductionAssetPath}.");
            Assert.That(tokens.SchemaVersion, Is.EqualTo(UiSemanticTokens.CurrentSchemaVersion));
            Assert.That(tokens.GetValidationIssues(), Is.Empty);
        }

        [Test]
        public void EverySemanticColorRoleIsPresentAndUnique()
        {
            UiSemanticTokens tokens = LoadProductionAsset();
            var values = new HashSet<uint>();

            foreach (UiColorRole role in Enum.GetValues(typeof(UiColorRole)))
            {
                Color32 color = tokens.Colors.Get(role);
                uint rgba = ((uint)color.r << 24) | ((uint)color.g << 16) | ((uint)color.b << 8) | color.a;
                Assert.That(values.Add(rgba), Is.True, $"Color role {role} duplicates another semantic color.");
            }

            Assert.That(values.Count, Is.EqualTo(Enum.GetValues(typeof(UiColorRole)).Length));
            Assert.That(tokens.Colors.Get(UiColorRole.Scrim).a, Is.InRange(0.75f, 0.85f));
            Assert.That(tokens.Colors.Get(UiColorRole.BlockedFeedback), Is.Not.EqualTo(tokens.Colors.Get(UiColorRole.FailureDanger)));
            Assert.That(tokens.Colors.Get(UiColorRole.Success), Is.Not.EqualTo(tokens.Colors.Get(UiColorRole.Leaf)));
        }

        [Test]
        public void EveryTypographyRoleHasAValidProjectFontReference()
        {
            UiSemanticTokens tokens = LoadProductionAsset();

            foreach (UiTypographyRole role in Enum.GetValues(typeof(UiTypographyRole)))
            {
                UiTypographyStyle style = tokens.Typography.Get(role);
                Assert.That(style, Is.Not.Null, $"Missing typography role {role}.");
                Assert.That(style.FontAsset, Is.Not.Null, $"Typography role {role} has no font asset.");
                Assert.That(AssetDatabase.Contains(style.FontAsset), Is.True, $"Typography role {role} uses a non-project reference.");
                Assert.That(AssetDatabase.GetAssetPath(style.FontAsset), Is.Not.Empty);
                Assert.That(style.SizeDp, Is.GreaterThanOrEqualTo(style.MinimumSizeDp));
                Assert.That(style.LineHeightPercent, Is.GreaterThanOrEqualTo(100f));
            }
        }

        [Test]
        public void SafeDefaultsMatchP1GeometryAndReducedMotionContracts()
        {
            UiSemanticTokens tokens = LoadProductionAsset();

            Assert.That(tokens.Layout.GridDp, Is.EqualTo(UiTokenDefaults.GridDp));
            Assert.That(tokens.Accessibility.MinimumTouchTargetDp, Is.EqualTo(TouchTargetValidator.DefaultMinimumSizeDp));
            Assert.That(tokens.Accessibility.ReferencePixelsPerDp, Is.EqualTo(TouchTargetValidator.DefaultReferencePixelsPerDp));
            Assert.That(tokens.Accessibility.ReferenceResolution, Is.EqualTo(UiTokenDefaults.ReferenceResolution));
            Assert.That(tokens.Accessibility.CanvasWidthMatch, Is.EqualTo(UiTokenDefaults.CanvasWidthMatch));
            Assert.That(tokens.Accessibility.ApplyDeviceSafeArea, Is.True);
            Assert.That(tokens.Accessibility.SafeAreaEdges, Is.EqualTo(SafeAreaEdges.All));
            Assert.That(tokens.Accessibility.SafeContentMarginDp, Is.GreaterThanOrEqualTo(UiTokenDefaults.SafeContentMarginDp));

            Assert.That(tokens.Motion.InstantSeconds, Is.Zero);
            Assert.That(tokens.Motion.ReducedPressSeconds, Is.LessThanOrEqualTo(tokens.Motion.PressSeconds));
            Assert.That(tokens.Motion.ReducedFeedbackSeconds, Is.LessThanOrEqualTo(tokens.Motion.FeedbackSeconds));
            Assert.That(tokens.Motion.ReducedTransitionSeconds, Is.LessThanOrEqualTo(tokens.Motion.TransitionSeconds));
            Assert.That(tokens.Motion.ReducedModalSeconds, Is.LessThanOrEqualTo(tokens.Motion.ModalSeconds));
            Assert.That(tokens.Motion.ReducedEmphasisSeconds, Is.LessThanOrEqualTo(tokens.Motion.EmphasisSeconds));
            Assert.That(tokens.Motion.ReducedMotionAllowsShake, Is.False);
            Assert.That(tokens.Motion.ReducedEasing, Is.EqualTo(UiEasing.Linear));

            Assert.That(tokens.Layers.WorldOrder, Is.LessThan(tokens.Layers.HudOrder));
            Assert.That(tokens.Layers.HudOrder, Is.LessThan(tokens.Layers.ScrimOrder));
            Assert.That(tokens.Layers.ScrimOrder, Is.LessThan(tokens.Layers.ModalOrder));
            Assert.That(tokens.Layers.ModalOrder, Is.LessThan(tokens.Layers.ToastOrder));
            Assert.That(tokens.Layers.ToastOrder, Is.LessThan(tokens.Layers.CriticalFeedbackOrder));
        }

        private static UiSemanticTokens LoadProductionAsset()
        {
            UiSemanticTokens tokens = AssetDatabase.LoadAssetAtPath<UiSemanticTokens>(
                UiSemanticTokens.ProductionAssetPath);
            Assert.That(tokens, Is.Not.Null);
            return tokens;
        }
    }
}
