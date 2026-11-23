using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public const string MUSIC_VOLUME_KEY = "MusicVolume";
    public const string SFX_VOLUME_KEY = "SFXVolume";
    private const float DEFAULT_MUSIC_VOLUME = 0.3F;
    private const float DEFAULT_SFX_VOLUME = 0.8F;
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip uiClickSFX;

    private float sfxVolume = 1f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        if (Instance != this) return;

        RestoreMusicVolume();
        sfxVolume = GetSavedSFXVolume();
        EnsureMusicPlaying();
    }

    public float GetSavedSFXVolume() => PlayerPrefs.GetFloat(SFX_VOLUME_KEY,DEFAULT_SFX_VOLUME);
    public float GetSavedMusicVolume() => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);

    public void RestoreMusicVolume()
    { 
        SetMusicVolume(GetSavedMusicVolume());
    }
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f); 
            sfxSource.PlayOneShot(clip, volumeScale* sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
    public void SaveMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY,volume);
        PlayerPrefs.Save();
        SetMusicVolume(volume);
    }
    public void SaveSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
        sfxVolume = volume;
    }

    public void PlayUIClick()
    {
        PlaySFX(uiClickSFX);
    }

    public void EnsureMusicPlaying()
    {
        if (musicSource != null && musicSource.clip !=null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}
