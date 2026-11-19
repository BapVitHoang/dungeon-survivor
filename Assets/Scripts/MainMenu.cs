using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private TMP_Text bestRecordText;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        Time.timeScale = 1.0f;

        LoadBestRecords();
        SetupVolumeSlider();
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
        if (volumeSlider == null) return;

        float savedVol = PlayerPrefs.GetFloat(AudioManager.MUSIC_VOLUME_KEY, 0.3f);
        volumeSlider.value = savedVol;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(AudioManager.MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
        AudioManager.Instance?.SetMusicVolume(value);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying =false;
#endif
    }
}
