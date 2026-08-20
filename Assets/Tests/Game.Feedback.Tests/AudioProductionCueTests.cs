using System.Collections.Generic;
using System.Reflection;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Events;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Services;
using Game.Feedback.Data;
using Game.Feedback.Presentation;
using Game.Flow.Events;
using Game.KnifeThrower.Events;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Feedback.Tests
{
    public sealed class AudioProductionCueTests
    {
        private const string SettingsPath =
            "Assets/Configurations/Systems/Audio/SoundSettings.asset";
        private const string OutputCatalogPath =
            "Assets/Configurations/Systems/Feedback/FeedbackOutputCatalog.asset";

        [Test]
        public void ProductionSettingsUseTheApprovedCueFamilyAndMixHierarchy()
        {
            SoundSettings settings = AssetDatabase.LoadAssetAtPath<SoundSettings>(SettingsPath);
            Assert.That(settings, Is.Not.Null);

            AssertClip(settings.KnifeThrowClip, "Knife_Throw_01", 0.2f, 0.4f);
            AssertClip(settings.KnifeLoadClip, "Knife_Load_01", 0.2f, 0.4f);
            AssertClip(settings.HitTargetClip, "Target_StickCrack_01", 0.2f, 0.4f);
            AssertClip(settings.HitKnifeClip, "Knife_Collision_Failure_01", 0.4f, 0.6f);
            AssertClip(settings.HitCollectibleClip, "Reward_Coin_01", 0.2f, 0.4f);
            AssertClip(settings.WinClip, "Target_Shatter_01", 0.2f, 0.4f);
            AssertClip(settings.LoseClip, "Failure_Accent_01", 0.2f, 0.4f);
            AssertClip(settings.ButtonClickClip, "UI_Click_01", 0.02f, 0.08f);
            AssertClip(settings.CurrencyGainClip, "Reward_Coin_01", 0.2f, 0.4f);
            AssertClip(settings.CurrencySpendClip, "Currency_Spend_01", 0.2f, 0.4f);

            Assert.That(settings.HitKnifeVolume, Is.GreaterThan(settings.HitTargetVolume));
            Assert.That(settings.HitKnifeVolume, Is.EqualTo(0.95f).Within(0.001f));
            Assert.That(settings.HitKnifeCooldownSeconds, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(settings.KnifeThrowCooldownSeconds, Is.EqualTo(0.08f).Within(0.001f));
            Assert.That(settings.HitTargetCooldownSeconds, Is.EqualTo(0.1f).Within(0.001f));

            FeedbackOutputCatalog catalog =
                AssetDatabase.LoadAssetAtPath<FeedbackOutputCatalog>(OutputCatalogPath);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.TryGetAudio(
                new FeedbackEventData(FeedbackKind.Navigation, "ui.press", "test_button"),
                out FeedbackAudioCue pressCue), Is.True);
            Assert.That(pressCue.Clip, Is.SameAs(settings.ButtonClickClip));
            Assert.That(pressCue.Volume, Is.EqualTo(0.55f).Within(0.001f));
        }

        [Test]
        public void KnifeAudioRoutesTypedImpactsOnceAndAppliesPerCueCooldowns()
        {
            var throwChannel = ScriptableObject.CreateInstance<KnifeThrownEventChannel>();
            var impactChannel = ScriptableObject.CreateInstance<KnifeImpactEventChannel>();
            var settings = ScriptableObject.CreateInstance<SoundSettings>();
            AudioClip throwClip = AudioClip.Create("throw", 32, 1, 8000, false);
            AudioClip targetClip = AudioClip.Create("target", 32, 1, 8000, false);
            AudioClip collisionClip = AudioClip.Create("collision", 32, 1, 8000, false);
            var player = new RecordingAudioPlayer();
            var clock = new FakeClock();
            var service = new KnifeAudioFeedbackService(
                player,
                settings,
                throwChannel,
                impactChannel,
                clock);

            try
            {
                Set(settings, nameof(SoundSettings.KnifeThrowClip), throwClip);
                Set(settings, nameof(SoundSettings.HitTargetClip), targetClip);
                Set(settings, nameof(SoundSettings.HitKnifeClip), collisionClip);
                Set(settings, nameof(SoundSettings.KnifeThrowVolume), 0.7f);
                Set(settings, nameof(SoundSettings.HitTargetVolume), 0.8f);
                Set(settings, nameof(SoundSettings.HitKnifeVolume), 0.95f);
                Set(settings, nameof(SoundSettings.KnifeThrowCooldownSeconds), 0.08f);
                Set(settings, nameof(SoundSettings.HitTargetCooldownSeconds), 0.1f);
                Set(settings, nameof(SoundSettings.HitKnifeCooldownSeconds), 0.55f);

                service.Start();
                service.Start();

                throwChannel.Raise();
                throwChannel.Raise();
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck));
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck));
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced));
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced));
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.OutOfView));

                Assert.That(player.Events, Has.Count.EqualTo(3));
                AssertEvent(player.Events[0], throwClip, 0.7f);
                AssertEvent(player.Events[1], targetClip, 0.8f);
                AssertEvent(player.Events[2], collisionClip, 0.95f);

                clock.NowValue = 0.08d;
                throwChannel.Raise();
                clock.NowValue = 0.1d;
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck));
                clock.NowValue = 0.55d;
                impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced));

                Assert.That(player.Events, Has.Count.EqualTo(6));
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(throwClip);
                Object.DestroyImmediate(targetClip);
                Object.DestroyImmediate(collisionClip);
                Object.DestroyImmediate(settings);
                Object.DestroyImmediate(impactChannel);
                Object.DestroyImmediate(throwChannel);
            }
        }

        [Test]
        public void CollisionOwnedFailureCueSuppressesGenericLevelFailureAccent()
        {
            var impactChannel = ScriptableObject.CreateInstance<KnifeImpactEventChannel>();
            var completedChannel = ScriptableObject.CreateInstance<LevelCompletedEventChannel>();
            var failedChannel = ScriptableObject.CreateInstance<LevelFailedEventChannel>();
            var settings = ScriptableObject.CreateInstance<SoundSettings>();
            AudioClip loseClip = AudioClip.Create("lose", 32, 1, 8000, false);
            var player = new RecordingAudioPlayer();
            var service = new LevelAudioFeedbackService(
                player,
                settings,
                impactChannel,
                completedChannel,
                failedChannel);

            try
            {
                Set(settings, nameof(SoundSettings.LoseClip), loseClip);
                Set(settings, nameof(SoundSettings.LoseVolume), 0.58f);
                service.Start();

                impactChannel.Raise(new KnifeImpactEventData(
                    KnifeImpactType.Bounced,
                    Vector2.zero,
                    countsAsFailure: true));
                failedChannel.Raise(null);
                Assert.That(player.Events, Is.Empty,
                    "The knife-impact service owns the collision audio; level failure must not double it.");

                failedChannel.Raise(null);
                Assert.That(player.Events, Has.Count.EqualTo(1),
                    "A later failure with no pending collision still receives the generic fallback accent.");
                AssertEvent(player.Events[0], loseClip, 0.58f);
            }
            finally
            {
                service.Dispose();
                Object.DestroyImmediate(loseClip);
                Object.DestroyImmediate(settings);
                Object.DestroyImmediate(failedChannel);
                Object.DestroyImmediate(completedChannel);
                Object.DestroyImmediate(impactChannel);
            }
        }

        private static void AssertClip(AudioClip clip, string expectedName, float minDuration, float maxDuration)
        {
            Assert.That(clip, Is.Not.Null, expectedName);
            Assert.That(clip.name, Is.EqualTo(expectedName));
            Assert.That(clip.length, Is.InRange(minDuration, maxDuration), expectedName);
        }

        private static void AssertEvent(PlaybackEvent playback, AudioClip clip, float volume)
        {
            Assert.That(playback.Clip, Is.SameAs(clip));
            Assert.That(playback.Volume, Is.EqualTo(volume).Within(0.001f));
        }

        private static void Set<T>(SoundSettings settings, string propertyName, T value)
        {
            FieldInfo field = typeof(SoundSettings).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, propertyName);
            field.SetValue(settings, value);
        }

        private sealed class FakeClock : IFeedbackPresentationClock
        {
            public double NowValue;
            public double Now => NowValue;
        }

        private sealed class RecordingAudioPlayer : IAudioPlayer
        {
            public List<PlaybackEvent> Events { get; } = new();

            public void PlayOneShot(AudioClip clip, float volume = 1f)
            {
                Events.Add(new PlaybackEvent(clip, volume));
            }
        }

        private readonly struct PlaybackEvent
        {
            public AudioClip Clip { get; }
            public float Volume { get; }

            public PlaybackEvent(AudioClip clip, float volume)
            {
                Clip = clip;
                Volume = volume;
            }
        }
    }
}
