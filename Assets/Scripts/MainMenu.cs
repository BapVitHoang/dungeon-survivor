using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private TMP_Text bestRecordText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GameObject quitButton;

    private void Start()
    {
        Time.timeScale = 1.0f;

        

        LoadBestRecords();
        SetupVolumeSlider();
        SetupSFXSlider();
#if UNITY_WEBGL
    if (quitButton != null)
            {
                quitButton.SetActive(false);
            }
#endif
    }

    private void LoadBestRecords()
    {
        if (bestRecordText == null) return;
        float bestTime = PlayerPrefs.GetFloat(GameManager.BEST_TIME_KEY, 0f);
        int bestKills = PlayerPrefs.GetInt(GameManager.BEST_KILLS_KEY, 0);
        int mins = Mathf.FloorToInt(bestTime / 60f);
        int secs = Mathf.FloorToInt(bestTime % 60f);

        bestRecordText.text = $"BEST RECORD: {mins:00}:{secs:00}  |  KILLS: {bestKills}";

    }

    private void SetupVolumeSlider()
    {
        if (volumeSlider != null)
        {
            float savedMusic = AudioManager.Instance != null
                ? AudioManager.Instance.GetSavedMusicVolume()
                : PlayerPrefs.GetFloat(AudioManager.MUSIC_VOLUME_KEY, 0.3f);
            volumeSlider.value = savedMusic;
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        }
    }

    private void SetupSFXSlider()
    {
        if (sfxSlider != null)
        {
            float savedSFX = AudioManager.Instance != null
                ? AudioManager.Instance.GetSavedSFXVolume()
                : PlayerPrefs.GetFloat(AudioManager.SFX_VOLUME_KEY, 0.8f);
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    public void OnVolumeChanged(float value)
    {
        AudioManager.Instance?.SaveMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SaveSFXVolume(value);
    }
    public void StartGame()
    {
        AudioManager.Instance?.EnsureMusicPlaying();
        AudioManager.Instance?.PlayUIClick();
        SceneManager.LoadScene("Gameplay");
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayUIClick();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying =false;
#endif
    }
}
