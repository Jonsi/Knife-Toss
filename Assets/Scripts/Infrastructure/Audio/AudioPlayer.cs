using KnifeToss.Core.Abstractions;
using UnityEngine;

namespace KnifeToss.Infrastructure.Audio
{
    public sealed class AudioPlayer : IAudioPlayer
    {
        private AudioSource sharedSource;

        public AudioPlayer()
        {
            ConfigureLowLatencyIfNeeded();
            sharedSource = CreateOrGetSource();
        }

        public void Play(AudioClip clip, Vector3 position, float volume)
        {
            PlayInternal(clip, position, volume);
        }

        public void Play2D(AudioClip clip)
        {
            PlayInternal(clip, Vector3.zero, 1f);
        }

        public void Play2D(AudioClip clip, float volume)
        {
            PlayInternal(clip, Vector3.zero, volume);
        }

        private void PlayInternal(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource source = sharedSource != null ? sharedSource : (sharedSource = CreateOrGetSource());
            if (source == null)
            {
                return;
            }

            source.transform.position = position;

            // Keep the source alive and simply fire a one-shot to avoid per-play allocations and reduce latency.
            source.PlayOneShot(clip, volume);
        }

        private static AudioSource CreateOrGetSource()
        {
            const string goName = "AudioPlayer (2D)";

            GameObject existing = GameObject.Find(goName);
            GameObject owner = existing != null ? existing : new GameObject(goName);

            AudioSource source = owner.GetComponent<AudioSource>();
            if (source == null)
            {
                source = owner.AddComponent<AudioSource>();
            }

            if (source == null)
            {
                // If we somehow still don't have a source (e.g., prefab stage restrictions), force a new owner.
                owner = new GameObject(goName);
                source = owner.AddComponent<AudioSource>();
            }

            if (source == null)
            {
                Debug.LogError("AudioPlayer could not create an AudioSource; SFX will be silent.");
                return null;
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f; // force 2D
            source.dopplerLevel = 0f;
            source.priority = 0; // highest priority, lowest latency
            source.spatialize = false;

            Object.DontDestroyOnLoad(owner);

            return source;
        }

        private static void ConfigureLowLatencyIfNeeded()
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();

            // 0 means let Unity pick the platform default; 256 is usually the smallest reliable buffer for mobile/desktop.
            const int targetBuffer = 256;
            if (config.dspBufferSize != targetBuffer && config.dspBufferSize > 0)
            {
                config.dspBufferSize = targetBuffer;
                AudioSettings.Reset(config);
            }
        }
    }
}
