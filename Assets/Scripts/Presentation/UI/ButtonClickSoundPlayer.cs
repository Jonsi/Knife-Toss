using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Events;
using UnityEngine;
using VContainer.Unity;

namespace KnifeToss.Presentation.UI
{





    public sealed class ButtonClickSoundPlayer : IStartable, System.IDisposable
    {
        private readonly IAudioPlayer audioPlayer;
        private readonly SoundSettings soundSettings;
        private readonly ButtonClickEventChannel buttonClickEventChannel;

        public ButtonClickSoundPlayer(IAudioPlayer audioPlayer, SoundSettings soundSettings, ButtonClickEventChannel buttonClickEventChannel)
        {
            this.audioPlayer = audioPlayer;
            this.soundSettings = soundSettings;
            this.buttonClickEventChannel = buttonClickEventChannel;
        }

        public void Start()
        {
            if (buttonClickEventChannel != null)
            {
                buttonClickEventChannel.Clicked += OnClicked;
            }
        }

        public void Dispose()
        {
            if (buttonClickEventChannel != null)
            {
                buttonClickEventChannel.Clicked -= OnClicked;
            }
        }

        private void OnClicked()
        {
            AudioClip clickSound = soundSettings != null ? soundSettings.ButtonClickSound : null;
            if (clickSound == null)
            {
                return;
            }

            audioPlayer.Play2D(clickSound);
        }
    }
}
