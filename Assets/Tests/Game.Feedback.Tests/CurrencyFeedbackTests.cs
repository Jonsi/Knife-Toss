using System.Collections.Generic;
using System.Reflection;
using Game.Economy.Events;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Services;
using Game.Feedback.Currency;
using NUnit.Framework;
using UnityEngine;

namespace Game.Feedback.Tests
{
    public class CurrencyFeedbackTests
    {
        private CurrencyBalanceChangedEventChannel _Channel;
        private SoundSettings _Settings;
        private AudioClip _GainClip;
        private AudioClip _SpendClip;
        private RecordingAudioPlayer _AudioPlayer;
        private CurrencyAudioFeedbackService _Service;

        [SetUp]
        public void SetUp()
        {
            _Channel = ScriptableObject.CreateInstance<CurrencyBalanceChangedEventChannel>();
            _Settings = ScriptableObject.CreateInstance<SoundSettings>();
            _GainClip = AudioClip.Create("currency-gain", 1, 1, 44100, false);
            _SpendClip = AudioClip.Create("currency-spend", 1, 1, 44100, false);
            SetClip(nameof(SoundSettings.CurrencyGainClip), _GainClip);
            SetClip(nameof(SoundSettings.CurrencySpendClip), _SpendClip);

            _AudioPlayer = new RecordingAudioPlayer();
            _Service = new CurrencyAudioFeedbackService(_AudioPlayer, _Settings, _Channel);
        }

        [TearDown]
        public void TearDown()
        {
            _Service?.Dispose();
            Object.DestroyImmediate(_Channel);
            Object.DestroyImmediate(_Settings);
            Object.DestroyImmediate(_GainClip);
            Object.DestroyImmediate(_SpendClip);
        }

        [TestCase(25, CurrencyFeedbackKind.Gain)]
        [TestCase(-25, CurrencyFeedbackKind.Spend)]
        [TestCase(0, CurrencyFeedbackKind.None)]
        public void Classify_UsesAmountChangedSign(int amountChanged, CurrencyFeedbackKind expected)
        {
            var data = new CurrencyBalanceChangedEventData { AmountChanged = amountChanged };

            Assert.That(CurrencyFeedbackSemantics.Classify(data), Is.EqualTo(expected));
        }

        [Test]
        public void GainEvent_PlaysConfiguredGainClip()
        {
            _Service.Start();

            Raise(amountChanged: 10);

            Assert.That(_AudioPlayer.PlayedClips, Is.EqualTo(new[] { _GainClip }));
        }

        [Test]
        public void SpendEvent_PlaysConfiguredSpendClip()
        {
            _Service.Start();

            Raise(amountChanged: -10);

            Assert.That(_AudioPlayer.PlayedClips, Is.EqualTo(new[] { _SpendClip }));
        }

        [Test]
        public void ZeroChange_DoesNotPlayAudio()
        {
            _Service.Start();

            Raise(amountChanged: 0);

            Assert.That(_AudioPlayer.PlayedClips, Is.Empty);
        }

        [Test]
        public void Start_IsIdempotent()
        {
            _Service.Start();
            _Service.Start();

            Raise(amountChanged: 10);

            Assert.That(_AudioPlayer.PlayedClips, Has.Count.EqualTo(1));
        }

        [Test]
        public void Dispose_UnsubscribesFromBalanceChanges()
        {
            _Service.Start();
            _Service.Dispose();

            Raise(amountChanged: 10);

            Assert.That(_AudioPlayer.PlayedClips, Is.Empty);
        }

        private void Raise(int amountChanged)
        {
            _Channel.Raise(new CurrencyBalanceChangedEventData
            {
                CurrencyId = "tomato_currency",
                PreviousBalance = 100,
                NewBalance = 100 + amountChanged,
                AmountChanged = amountChanged
            });
        }

        private void SetClip(string propertyName, AudioClip clip)
        {
            FieldInfo field = typeof(SoundSettings).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized backing field for {propertyName}.");
            field.SetValue(_Settings, clip);
        }

        private sealed class RecordingAudioPlayer : IAudioPlayer
        {
            public List<AudioClip> PlayedClips { get; } = new();

            public void PlayOneShot(AudioClip clip, float volume = 1f)
            {
                PlayedClips.Add(clip);
            }
        }
    }
}
