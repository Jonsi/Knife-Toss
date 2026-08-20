using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Services;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Feedback.Haptics;
using Game.Feedback.Presentation;
using Game.Feedback.VFX.Services;
using Game.UI.Common;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Feedback.Tests
{
    public class FeedbackOutputHooksTests
    {
        [Test]
        public void ProductionCatalogIsWiredAndContainsUniqueExactMappings()
        {
            const string catalogPath =
                "Assets/Configurations/Systems/Feedback/FeedbackOutputCatalog.asset";
            FeedbackOutputCatalog catalog =
                AssetDatabase.LoadAssetAtPath<FeedbackOutputCatalog>(catalogPath);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Mappings, Has.Count.EqualTo(12));

            var keys = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            foreach (FeedbackOutputMapping mapping in catalog.Mappings)
            {
                Assert.That(mapping, Is.Not.Null);
                Assert.That(mapping.AudioClip, Is.Not.Null, mapping.CueId);
                Assert.That(keys.Add($"{mapping.Kind}:{mapping.CueId}"), Is.True, mapping.CueId);

                if (mapping.Kind == FeedbackKind.Navigation &&
                    mapping.CueId == ActionButtonPresenter.PressFeedbackCueId)
                {
                    Assert.That(mapping.VfxCueId, Is.Empty);
                    Assert.That(mapping.HapticStyle, Is.EqualTo(FeedbackHapticStyle.None));
                }
                else
                {
                    Assert.That(mapping.VfxCueId, Is.Not.Empty, mapping.CueId);
                    Assert.That(mapping.HapticStyle, Is.Not.EqualTo(FeedbackHapticStyle.None), mapping.CueId);
                }
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/GameInstaller.prefab");
            Assert.That(prefab, Is.Not.Null);
            MonoBehaviour installer = null;
            foreach (MonoBehaviour component in prefab.GetComponents<MonoBehaviour>())
            {
                if (component != null && component.GetType().FullName == "Game.Installers.GameInstaller")
                {
                    installer = component;
                    break;
                }
            }
            Assert.That(installer, Is.Not.Null);
            var serialized = new SerializedObject(installer);
            Assert.That(
                serialized.FindProperty("_FeedbackOutputCatalog").objectReferenceValue,
                Is.SameAs(catalog));
        }

        [Test]
        public void CatalogUsesExactKindAndOrdinalCueMappingWithoutFallback()
        {
            var catalog = ScriptableObject.CreateInstance<FeedbackOutputCatalog>();
            AudioClip clip = AudioClip.Create("Blocked", 32, 1, 8000, false);
            try
            {
                catalog.SetMappingsForTests(new FeedbackOutputMapping(
                    FeedbackKind.Blocked,
                    "action.blocked",
                    clip,
                    0.65f,
                    "ui.blocked",
                    FeedbackHapticStyle.Light));

                var mapped = new FeedbackEventData(FeedbackKind.Blocked, "action.blocked");
                Assert.That(catalog.TryGetAudio(mapped, out FeedbackAudioCue audio), Is.True);
                Assert.That(audio.Clip, Is.SameAs(clip));
                Assert.That(audio.Volume, Is.EqualTo(0.65f).Within(0.001f));
                Assert.That(catalog.TryGetVfx(mapped, out FeedbackVfxCue vfx), Is.True);
                Assert.That(vfx.CueId, Is.EqualTo("ui.blocked"));
                Assert.That(catalog.TryGetHaptic(mapped, out FeedbackHapticStyle haptic), Is.True);
                Assert.That(haptic, Is.EqualTo(FeedbackHapticStyle.Light));

                Assert.That(
                    catalog.TryGetAudio(
                        new FeedbackEventData(FeedbackKind.Success, "action.blocked"),
                        out _),
                    Is.False);
                Assert.That(
                    catalog.TryGetVfx(
                        new FeedbackEventData(FeedbackKind.Blocked, "Action.Blocked"),
                        out _),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void SharedEventFansOutIndependentlyAndUsesDuplicateCooldownPerContext()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var catalog = ScriptableObject.CreateInstance<FeedbackOutputCatalog>();
            AudioClip clip = AudioClip.Create("Blocked", 32, 1, 8000, false);
            var clock = new FakeClock();
            var audio = new RecordingAudioPlayer();
            var vfx = new RecordingVfxPlayer();
            var haptics = new RecordingHapticPlayer();
            var audioPresenter = new FeedbackAudioPresenter(channel, catalog, audio, clock);
            var vfxPresenter = new FeedbackVfxPresenter(channel, catalog, vfx, clock);
            var hapticPresenter = new FeedbackHapticPresenter(channel, catalog, haptics, clock);

            try
            {
                catalog.SetMappingsForTests(new FeedbackOutputMapping(
                    FeedbackKind.Blocked,
                    "action.blocked",
                    clip,
                    0.75f,
                    "ui.blocked",
                    FeedbackHapticStyle.Medium));
                audioPresenter.Start();
                audioPresenter.Start();
                vfxPresenter.Start();
                hapticPresenter.Start();

                channel.Raise(Event("one"));
                AssertCounts(audio, vfx, haptics, 1);

                clock.NowValue = 0.1d;
                channel.Raise(Event("one"));
                AssertCounts(audio, vfx, haptics, 1);

                channel.Raise(Event("two"));
                AssertCounts(audio, vfx, haptics, 2);

                clock.NowValue = 0.75d;
                channel.Raise(Event("one"));
                AssertCounts(audio, vfx, haptics, 3);
                Assert.That(audio.LastVolume, Is.EqualTo(0.75f).Within(0.001f));
                Assert.That(vfx.LastCue.CueId, Is.EqualTo("ui.blocked"));
                Assert.That(haptics.LastStyle, Is.EqualTo(FeedbackHapticStyle.Medium));

                audioPresenter.Dispose();
                clock.NowValue = 2d;
                channel.Raise(Event("three"));
                Assert.That(audio.Count, Is.EqualTo(3));
                Assert.That(vfx.Count, Is.EqualTo(4));
                Assert.That(haptics.Count, Is.EqualTo(4));
            }
            finally
            {
                audioPresenter.Dispose();
                vfxPresenter.Dispose();
                hapticPresenter.Dispose();
                Object.DestroyImmediate(clip);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(channel);
            }
        }

        [Test]
        public void UnmappedAndNullConfigurationsAreSilentAndTeardownStopsRouting()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var catalog = ScriptableObject.CreateInstance<FeedbackOutputCatalog>();
            var clock = new FakeClock();
            var audio = new RecordingAudioPlayer();
            var vfx = new RecordingVfxPlayer();
            var haptics = new RecordingHapticPlayer();
            var audioPresenter = new FeedbackAudioPresenter(channel, null, audio, clock);
            var vfxPresenter = new FeedbackVfxPresenter(channel, catalog, vfx, clock);
            var hapticPresenter = new FeedbackHapticPresenter(channel, catalog, haptics, clock);

            try
            {
                Assert.DoesNotThrow(() =>
                {
                    audioPresenter.Start();
                    vfxPresenter.Start();
                    hapticPresenter.Start();
                    channel.Raise(new FeedbackEventData(FeedbackKind.Reward, "reward.unknown"));
                });
                AssertCounts(audio, vfx, haptics, 0);

                audioPresenter.Dispose();
                vfxPresenter.Dispose();
                hapticPresenter.Dispose();
                Assert.DoesNotThrow(() => channel.Raise(Event("after_teardown")));
                AssertCounts(audio, vfx, haptics, 0);
            }
            finally
            {
                audioPresenter.Dispose();
                vfxPresenter.Dispose();
                hapticPresenter.Dispose();
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(channel);
            }
        }

        [Test]
        public void RestartAfterTeardownClearsCooldownState()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var catalog = ScriptableObject.CreateInstance<FeedbackOutputCatalog>();
            var player = new RecordingHapticPlayer();
            var presenter = new FeedbackHapticPresenter(channel, catalog, player, new FakeClock());
            try
            {
                catalog.SetMappingsForTests(new FeedbackOutputMapping(
                    FeedbackKind.Blocked,
                    "action.blocked",
                    hapticStyle: FeedbackHapticStyle.Light));

                presenter.Start();
                channel.Raise(Event("same"));
                presenter.Dispose();
                presenter.Start();
                channel.Raise(Event("same"));

                Assert.That(player.Count, Is.EqualTo(2));
            }
            finally
            {
                presenter.Dispose();
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(channel);
            }
        }

        [Test]
        public void VfxSignalDeduplicatesSubscriptionAndUnsubscribesDeterministically()
        {
            var signal = new FeedbackVfxSignal();
            int count = 0;
            void OnRequested(FeedbackVfxCue _, FeedbackEventData __) => count++;

            signal.Subscribe(OnRequested);
            signal.Subscribe(OnRequested);
            signal.Play(new FeedbackVfxCue("ui.blocked"), Event("one"));
            Assert.That(count, Is.EqualTo(1));

            signal.Unsubscribe(OnRequested);
            signal.Play(new FeedbackVfxCue("ui.blocked"), Event("two"));
            Assert.That(count, Is.EqualTo(1));
        }

        private static FeedbackEventData Event(string contextId)
        {
            return new FeedbackEventData(FeedbackKind.Blocked, "action.blocked", contextId);
        }

        private static void AssertCounts(
            RecordingAudioPlayer audio,
            RecordingVfxPlayer vfx,
            RecordingHapticPlayer haptics,
            int expected)
        {
            Assert.That(audio.Count, Is.EqualTo(expected));
            Assert.That(vfx.Count, Is.EqualTo(expected));
            Assert.That(haptics.Count, Is.EqualTo(expected));
        }

        private sealed class FakeClock : IFeedbackPresentationClock
        {
            public double NowValue;
            public double Now => NowValue;
        }

        private sealed class RecordingAudioPlayer : IAudioPlayer
        {
            public int Count { get; private set; }
            public float LastVolume { get; private set; }

            public void PlayOneShot(AudioClip clip, float volume = 1f)
            {
                Count++;
                LastVolume = volume;
            }
        }

        private sealed class RecordingVfxPlayer : IFeedbackVfxPlayer
        {
            public int Count { get; private set; }
            public FeedbackVfxCue LastCue { get; private set; }

            public void Play(FeedbackVfxCue cue, FeedbackEventData source)
            {
                Count++;
                LastCue = cue;
            }
        }

        private sealed class RecordingHapticPlayer : IHapticFeedbackPlayer
        {
            public int Count { get; private set; }
            public FeedbackHapticStyle LastStyle { get; private set; }

            public void Play(FeedbackHapticStyle style)
            {
                Count++;
                LastStyle = style;
            }
        }
    }
}
