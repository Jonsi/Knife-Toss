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
            PlayInternal(clip, position, volume, 0f);
        }

        public void Play(SoundEffect sound, Vector3 position, float volume)
        {
            if (sound == null || !sound.HasClip)
            {
                return;
            }

            PlayInternal(sound.Clip, position, volume, sound.StartOffsetSeconds);
        }

        private void PlayInternal(AudioClip clip, Vector3 position, float volume, float offsetSeconds)
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

            if (offsetSeconds <= 0.0001f)
            {
                // Keep the source alive and simply fire a one-shot to avoid per-play allocations and reduce latency.
                source.PlayOneShot(clip, volume);
                return;
            }

            // Start partway into the clip to skip leading silence; note this source can only play one clip at a time.
            source.clip = clip;
            source.volume = volume;
            source.loop = false;

            int offsetSamples = Mathf.Min(Mathf.FloorToInt(offsetSeconds * clip.frequency), clip.samples - 1);
            source.timeSamples = Mathf.Max(0, offsetSamples);
            source.Play();
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
