using System;
using System.Collections.Generic;
using FlatIronFields.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace FlatIronFields.Game
{
    /// <summary>
    /// Procedural audio service. All clips are generated in memory, so the Unity
    /// build needs no external audio assets. Music and SFX are routed through
    /// the generated AudioMixer groups when available.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        private const int SampleRate = 22050;

        public AudioMixer Mixer;
        public AudioMixerGroup MusicGroup;
        public AudioMixerGroup SfxGroup;
        [Range(0f, 1f)] public float MusicVolume = 0.18f;
        [Range(0f, 1f)] public float SfxVolume = 0.42f;

        private readonly Dictionary<AudioCue, AudioClip> cueClips = new Dictionary<AudioCue, AudioClip>();
        private AudioSource musicSource;
        private AudioSource ambienceSource;
        private AudioSource engineSource;
        private AudioSource rainSource;
        private AudioSource sfxSource;
        private AudioClip musicLoop;
        private AudioClip ambienceLoop;
        private AudioClip engineLoop;
        private AudioClip rainLoop;
        private float targetEngineVolume;
        private float targetEnginePitch = 1f;
        private Weather weather = Weather.Sunny;

        private void Awake()
        {
            Build();
        }

        private void Update()
        {
            if (engineSource == null) { return; }
            engineSource.volume = Mathf.Lerp(engineSource.volume, targetEngineVolume, Time.deltaTime * 4f);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetEnginePitch, Time.deltaTime * 4f);
        }

        public void Initialize(GameState state)
        {
            Build();
            if (state == null) { return; }
            SetWeather(state.Weather);
        }

        public void SetWeather(Weather value)
        {
            weather = value;
            if (rainSource == null) { return; }
            rainSource.enabled = value == Weather.Rainy;
            if (ambienceSource != null)
            {
                ambienceSource.volume = value == Weather.Cloudy ? 0.05f : 0.08f;
            }
        }

        public void SetTractorMovement(bool moving, float speedRatio)
        {
            targetEngineVolume = moving ? 0.22f : 0.035f;
            targetEnginePitch = 0.82f + Mathf.Clamp01(speedRatio) * 0.45f;
        }

        public void Play(AudioCue cue)
        {
            if (sfxSource == null || !cueClips.TryGetValue(cue, out AudioClip clip) || clip == null) { return; }
            sfxSource.PlayOneShot(clip, SfxVolume);
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            if (musicSource != null) { musicSource.volume = MusicVolume; }
            ApplyMixerVolume("MusicVolume", MusicVolume);
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null) { sfxSource.volume = SfxVolume; }
            ApplyMixerVolume("SfxVolume", SfxVolume);
        }

        private void Build()
        {
            if (musicSource != null) { return; }

            musicLoop = CreateLoop("FarmMusic", 8f, SampleMusic);
            ambienceLoop = CreateLoop("FarmAmbience", 4f, SampleAmbience);
            engineLoop = CreateLoop("TractorEngine", 1f, SampleEngine);
            rainLoop = CreateLoop("Rain", 2f, SampleRain);

            cueClips.Clear();
            cueClips[AudioCue.Till] = CreateCue("Till", 0.18f, 170f, 12f, 0.25f);
            cueClips[AudioCue.Plant] = CreateCue("Plant", 0.14f, 420f, 18f, 0.05f);
            cueClips[AudioCue.Water] = CreateCue("Water", 0.30f, 300f, 7f, 0.55f);
            cueClips[AudioCue.Harvest] = CreateCue("Harvest", 0.22f, 620f, 12f, 0.10f);
            cueClips[AudioCue.Coin] = CreateCue("Coin", 0.28f, 880f, 9f, 0.02f);
            cueClips[AudioCue.Build] = CreateCue("Build", 0.35f, 130f, 6f, 0.30f);
            cueClips[AudioCue.Upgrade] = CreateCue("Upgrade", 0.40f, 540f, 5f, 0.03f);
            cueClips[AudioCue.Reward] = CreateCue("Reward", 0.45f, 720f, 4f, 0.02f);
            cueClips[AudioCue.Day] = CreateCue("Day", 0.32f, 360f, 6f, 0.04f);
            cueClips[AudioCue.Attachment] = CreateCue("Attachment", 0.20f, 240f, 9f, 0.20f);
            cueClips[AudioCue.Rain] = CreateCue("Rain cue", 0.25f, 190f, 5f, 0.70f);
            cueClips[AudioCue.Sun] = CreateCue("Sun cue", 0.25f, 680f, 6f, 0.02f);

            musicSource = AddSource("Music", musicLoop, MusicGroup, MusicVolume, true);
            ambienceSource = AddSource("Ambience", ambienceLoop, MusicGroup, 0.08f, true);
            engineSource = AddSource("TractorEngine", engineLoop, SfxGroup, 0.035f, true);
            rainSource = AddSource("Rain", rainLoop, SfxGroup, 0.12f, true);
            sfxSource = AddSource("Sfx", null, SfxGroup, SfxVolume, false);
            rainSource.enabled = weather == Weather.Rainy;
            ApplyMixerVolume("MusicVolume", MusicVolume);
            ApplyMixerVolume("SfxVolume", SfxVolume);
        }

        private AudioSource AddSource(string name, AudioClip clip, AudioMixerGroup group, float volume, bool loop)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = group;
            source.volume = volume;
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            if (clip != null && loop) { source.Play(); }
            return source;
        }

        private void ApplyMixerVolume(string parameter, float normalized)
        {
            if (Mixer == null) { return; }
            float db = normalized <= 0.001f ? -80f : 20f * Mathf.Log10(normalized);
            Mixer.SetFloat(parameter, db);
        }

        private static AudioClip CreateLoop(string name, float duration, Func<float, float> sample)
        {
            int samples = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
            float[] data = new float[samples];
            float fade = Mathf.Min(0.1f, duration * 0.2f);
            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)SampleRate;
                float envelope = Mathf.Min(1f, Mathf.Min(time, duration - time) / fade);
                data[i] = sample(time) * envelope;
            }
            AudioClip clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateCue(string name, float duration, float frequency, float decay, float noiseMix)
        {
            int samples = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
            float[] data = new float[samples];
            var random = new System.Random(name.GetHashCode());
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Exp(-t * decay);
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t);
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                data[i] = Mathf.Clamp((tone * (1f - noiseMix) + noise * noiseMix) * envelope * 0.35f, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float SampleMusic(float time)
        {
            float[] notes = { 261.63f, 329.63f, 392.00f, 493.88f, 392.00f, 329.63f, 293.66f, 261.63f };
            int index = Mathf.FloorToInt(time) % notes.Length;
            float phase = time - Mathf.Floor(time);
            float envelope = Mathf.Sin(Mathf.PI * phase);
            float fundamental = Mathf.Sin(2f * Mathf.PI * notes[index] * time);
            float harmony = Mathf.Sin(2f * Mathf.PI * notes[index] * 1.5f * time) * 0.25f;
            return (fundamental + harmony) * envelope * 0.13f;
        }

        private static float SampleAmbience(float time)
        {
            float wind = Mathf.Sin(2f * Mathf.PI * 0.23f * time) * 0.35f;
            float bird = Mathf.Sin(2f * Mathf.PI * 1200f * time) * Mathf.Exp(-6f * (time % 2f)) * 0.08f;
            return (wind + bird) * 0.12f;
        }

        private static float SampleEngine(float time)
        {
            float baseTone = Mathf.Sin(2f * Mathf.PI * 48f * time) * 0.55f;
            float harmonic = Mathf.Sin(2f * Mathf.PI * 96f * time) * 0.25f;
            float pulse = Mathf.Sin(2f * Mathf.PI * 12f * time) > 0f ? 1f : -1f;
            return (baseTone + harmonic + pulse * 0.08f) * 0.25f;
        }

        private static float SampleRain(float time)
        {
            int seed = Mathf.FloorToInt(time * SampleRate);
            float noise = (float)(new System.Random(seed).NextDouble() * 2.0 - 1.0);
            return noise * 0.16f;
        }
    }
}
