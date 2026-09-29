using UnityEngine;

public class AudioPool : MonoBehaviour {
    public static AudioPool Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<AudioPool>();
                if (_instance == null) {
                    GameObject go = new GameObject("AudioPool");
                    _instance = go.AddComponent<AudioPool>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    private static AudioPool _instance;

    public AudioClip hitSound;
    public AudioClip painSound;
    public AudioClip bgmClip;

    private const int VOICE_COUNT = 6;
    private AudioSource[] voiceSources;
    private AudioSource bgmSource;
    private int currentVoiceIndex = 0;
    private bool isInitialized = false;

    void Awake() {
        if (_instance == null || _instance == this) {
            _instance = this;
            InitializeAudioSources();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources() {
        if (isInitialized) return;
        isInitialized = true;
        voiceSources = new AudioSource[VOICE_COUNT];
        for (int i = 0; i < VOICE_COUNT; i++) {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            voiceSources[i] = source;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        if (bgmClip != null) {
            PlayBGM(bgmClip);
        }
    }

    public void Play(AudioClip clip, float volume = 1f) {
        if (clip == null || voiceSources == null) return;

        currentVoiceIndex = (currentVoiceIndex + 1) % VOICE_COUNT;
        AudioSource source = voiceSources[currentVoiceIndex];

        // Busy guard: drop sound request on voice starvation instead of clipping active audio
        if (source.isPlaying) {
            return;
        }

        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.Play();
    }

    public void PlayHit() {
        if (hitSound != null) {
            Play(hitSound, 1f);
        }
    }

    public void PlayPain() {
        if (painSound != null) {
            Play(painSound, 1f);
        }
    }

    public void PlayBGM(AudioClip clip = null, float volume = 0.8f) {
        if (clip != null) {
            bgmClip = clip;
        }

        if (bgmSource != null && bgmClip != null) {
            bgmSource.clip = bgmClip;
            bgmSource.volume = Mathf.Clamp01(volume);
            bgmSource.Play();
        }
    }

    public void StopBGM() {
        if (bgmSource != null && bgmSource.isPlaying) {
            bgmSource.Stop();
        }
    }
}
