using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Audio
{
    /// <summary>
    /// Master audio manager providing high-fidelity slot sound effects, background music,
    /// and dynamic procedural waveform synthesis fallbacks. Guarantees complete sound in WebGL
    /// without relying on external audio assets.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Audio Clips (Optional - Uses Procedural Synth if Empty)")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip betChangeClip;
        [SerializeField] private AudioClip spinStartClip;
        [SerializeField] private AudioClip reelTickClip;
        [SerializeField] private AudioClip reelStopClip;
        [SerializeField] private AudioClip winSmallClip;
        [SerializeField] private AudioClip winBigClip;
        [SerializeField] private AudioClip winMegaClip;
        [SerializeField] private AudioClip freeSpinsClip;
        [SerializeField] private AudioClip coinsCollectClip;
        [SerializeField] private AudioClip anticipationClip;
        [SerializeField] private AudioClip gambleCardFlipClip;
        [SerializeField] private AudioClip gambleWinClip;
        [SerializeField] private AudioClip gambleLoseClip;

        private readonly Dictionary<SoundType, AudioClip> _synthClips = new Dictionary<SoundType, AudioClip>();
        private bool _isMuted = false;

        public bool IsMuted => _isMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            GenerateProceduralAudioFallbacks();
        }

        public void PlaySound(SoundType type, float volume = 1f)
        {
            if (_isMuted || sfxSource == null) return;

            AudioClip clip = GetClip(type);
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void ToggleMute()
        {
            _isMuted = !_isMuted;
            if (sfxSource != null) sfxSource.mute = _isMuted;
            if (musicSource != null) musicSource.mute = _isMuted;
        }

        private AudioClip GetClip(SoundType type)
        {
            return type switch
            {
                SoundType.ButtonClick => buttonClickClip != null ? buttonClickClip : _synthClips.GetValueOrDefault(type),
                SoundType.BetChange => betChangeClip != null ? betChangeClip : _synthClips.GetValueOrDefault(type),
                SoundType.SpinStart => spinStartClip != null ? spinStartClip : _synthClips.GetValueOrDefault(type),
                SoundType.ReelTick => reelTickClip != null ? reelTickClip : _synthClips.GetValueOrDefault(type),
                SoundType.ReelStop => reelStopClip != null ? reelStopClip : _synthClips.GetValueOrDefault(type),
                SoundType.WinSmall => winSmallClip != null ? winSmallClip : _synthClips.GetValueOrDefault(type),
                SoundType.WinBig => winBigClip != null ? winBigClip : _synthClips.GetValueOrDefault(type),
                SoundType.WinMega => winMegaClip != null ? winMegaClip : _synthClips.GetValueOrDefault(type),
                SoundType.FreeSpinsTrigger => freeSpinsClip != null ? freeSpinsClip : _synthClips.GetValueOrDefault(type),
                SoundType.CoinsCollect => coinsCollectClip != null ? coinsCollectClip : _synthClips.GetValueOrDefault(type),
                SoundType.Anticipation => anticipationClip != null ? anticipationClip : _synthClips.GetValueOrDefault(type),
                SoundType.GambleCardFlip => gambleCardFlipClip != null ? gambleCardFlipClip : _synthClips.GetValueOrDefault(type),
                SoundType.GambleWin => gambleWinClip != null ? gambleWinClip : _synthClips.GetValueOrDefault(type),
                SoundType.GambleLose => gambleLoseClip != null ? gambleLoseClip : _synthClips.GetValueOrDefault(type),
                _ => null
            };
        }

        private void GenerateProceduralAudioFallbacks()
        {
            _synthClips[SoundType.ButtonClick] = CreateToneClip("Click", 880f, 0.04f, WaveType.Square);
            _synthClips[SoundType.BetChange] = CreateToneClip("BetChange", 523f, 0.06f, WaveType.Triangle);
            _synthClips[SoundType.SpinStart] = CreateSlideClip("SpinStart", 220f, 660f, 0.22f);
            _synthClips[SoundType.ReelTick] = CreateToneClip("Tick", 350f, 0.025f, WaveType.Noise);
            _synthClips[SoundType.ReelStop] = CreateSlideClip("ReelStop", 280f, 180f, 0.08f);
            _synthClips[SoundType.WinSmall] = CreateArpeggioClip("WinSmall", new float[] { 523f, 659f, 784f, 1046f }, 0.08f);
            _synthClips[SoundType.WinBig] = CreateArpeggioClip("WinBig", new float[] { 523f, 659f, 784f, 1046f, 1318f, 1568f }, 0.12f);
            _synthClips[SoundType.WinMega] = CreateArpeggioClip("WinMega", new float[] { 523f, 659f, 784f, 1046f, 1318f, 1568f, 2093f }, 0.14f);
            _synthClips[SoundType.FreeSpinsTrigger] = CreateArpeggioClip("FreeSpins", new float[] { 440f, 554f, 659f, 880f, 1108f }, 0.15f);
            _synthClips[SoundType.CoinsCollect] = CreateToneClip("Coins", 1200f, 0.05f, WaveType.Sine);
            _synthClips[SoundType.Anticipation] = CreateSlideClip("Anticipation", 320f, 880f, 0.45f);
            _synthClips[SoundType.GambleCardFlip] = CreateSlideClip("CardFlip", 400f, 750f, 0.12f);
            _synthClips[SoundType.GambleWin] = CreateArpeggioClip("GambleWin", new float[] { 587f, 740f, 880f, 1174f }, 0.09f);
            _synthClips[SoundType.GambleLose] = CreateSlideClip("GambleLose", 320f, 130f, 0.35f);
        }

        private enum WaveType { Sine, Square, Triangle, Noise }

        private AudioClip CreateToneClip(string clipName, float frequency, float duration, WaveType wave)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / totalSamples);
                float sample = wave switch
                {
                    WaveType.Sine => Mathf.Sin(2f * Mathf.PI * frequency * t),
                    WaveType.Square => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * t)),
                    WaveType.Triangle => Mathf.PingPong(t * frequency * 4f, 2f) - 1f,
                    WaveType.Noise => (Random.value * 2f - 1f) * Mathf.Exp(-t * 80f),
                    _ => 0f
                };

                samples[i] = sample * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateSlideClip(string clipName, float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];
            float phase = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float progress = (float)i / totalSamples;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, progress);
                float envelope = 1f - (progress * 0.5f);

                phase += 2f * Mathf.PI * currentFreq / sampleRate;
                samples[i] = Mathf.Sin(phase) * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateArpeggioClip(string clipName, float[] notes, float noteDuration)
        {
            int sampleRate = 44100;
            int noteSamples = Mathf.FloorToInt(sampleRate * noteDuration);
            int totalSamples = noteSamples * notes.Length;
            float[] samples = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                float freq = notes[n];
                int startIdx = n * noteSamples;

                for (int i = 0; i < noteSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - ((float)i / noteSamples);
                    float sample = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
                    samples[startIdx + i] = sample * envelope * 0.35f;
                }
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
