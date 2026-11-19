using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public const string MUSIC_VOLUME_KEY = "MusicVolume";
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

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
        }
    }
    private void Start()
    {
        RestoreMusicVolume();
    }

    public void RestoreMusicVolume()
    {
        float savedVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY,0.3f);
        SetMusicVolume(savedVol);
    }
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f); 
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
}
