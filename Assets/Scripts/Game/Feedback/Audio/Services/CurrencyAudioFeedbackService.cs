using System;
using Game.Economy.Events;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Currency;
using UnityEngine;
using VContainer.Unity;

namespace Game.Feedback.Audio.Services
{
    /// <summary>
    /// Presents currency gain and spend sounds from the existing balance event channel.
    /// Economy services remain unaware of audio presentation.
    /// </summary>
    public sealed class CurrencyAudioFeedbackService : IStartable, IDisposable
    {
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly CurrencyBalanceChangedEventChannel _BalanceChangedChannel;
        private bool _IsStarted;

        public CurrencyAudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            CurrencyBalanceChangedEventChannel balanceChangedChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _BalanceChangedChannel = balanceChangedChannel;
        }

        public void Start()
        {
            if (_IsStarted)
            {
                return;
            }

            _BalanceChangedChannel?.Subscribe(OnBalanceChanged);
            _IsStarted = true;
        }

        public void Dispose()
        {
            if (!_IsStarted)
            {
                return;
            }

            _BalanceChangedChannel?.Unsubscribe(OnBalanceChanged);
            _IsStarted = false;
        }

        private void OnBalanceChanged(CurrencyBalanceChangedEventData data)
        {
            AudioClip clip = CurrencyFeedbackSemantics.Classify(data) switch
            {
                CurrencyFeedbackKind.Gain => _SoundSettings?.CurrencyGainClip,
                CurrencyFeedbackKind.Spend => _SoundSettings?.CurrencySpendClip,
                _ => null
            };

            if (clip != null)
            {
                _AudioPlayer?.PlayOneShot(clip);
            }
        }
    }
}
