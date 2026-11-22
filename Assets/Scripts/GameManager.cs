using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public enum GameState
{
    Playing,
    Paused,
    Ended
}
public class GameManager : MonoBehaviour
{
    public const string BEST_TIME_KEY = "BestTime";
    public const string BEST_KILLS_KEY = "BestKills";

    public GameState State { get; private set; } = GameState.Playing;

    [Header("Gameplay References")]
    [SerializeField] private float survivalTime = 120f;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerLevel playerLevel;
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("HUD UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text killCountText;


    [Header("End & Pause Panels")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private GameObject pausePanel;

    [Header("Audio")]
    [SerializeField] private AudioClip victorySFX;
    [SerializeField] private AudioClip gameOverSFX;

    private float remainingTime;
    private int killCount = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        remainingTime = survivalTime;

        if (upgradeManager == null )
        {
            upgradeManager = FindAnyObjectByType<UpgradeManager>();
        }
        if (playerHealth != null)
        {
            playerHealth.OnDied += HandlePlayerDied;
        }
        Enemy.OnEliteEnemyDied += HandleEliteDied;
        Enemy.OnAnyEnemyDied += HandleEnemyDied;

        if (endPanel != null) endPanel.SetActive(false);
        
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdateKillUI();
        UpdateTimerUI();
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnDied -= HandlePlayerDied;
        Enemy.OnAnyEnemyDied -= HandleEnemyDied;
         Enemy.OnEliteEnemyDied -= HandleEliteDied;
 
    }
    // Update is called once per frame
    private void Update()
    {
       
        if (State == GameState.Ended)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (State != GameState.Playing) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <=0)
        {
            remainingTime = 0;
            EndGame("YOU SURVIVED!",true);
        }

        UpdateTimerUI();
    }

    private void HandleEnemyDied()
    {
        killCount++;
        UpdateKillUI();
    }

    private void HandleEliteDied()
    {
        CameraFollow.Instance?.TriggerShake(0.22f, 0.28f);
    }
    private void UpdateKillUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + killCount;
        }
    }


    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void EndGame(string titleMessage, bool isVictory)
    {
        if (State == GameState.Ended) return;
        State = GameState.Ended;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        AudioClip endClip = isVictory ? victorySFX : gameOverSFX;
        AudioManager.Instance?.PlaySFX(endClip, 0.8f);
        AudioManager.Instance?.SetMusicVolume(0.08f);
        Time.timeScale = 0f;

        float survivedTime = survivalTime - remainingTime;
        bool isNewRecord = SaveBestRecords(survivedTime, killCount);

        if (endPanel != null)
        {
            endPanel.SetActive(true);
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (resultText != null)
            {
                resultText.text = titleMessage;
                resultText.color = isVictory? Palette.AccentGold : Palette.HealthCrimson;
            }

            if (summaryText != null)
            {
                int mins = Mathf.FloorToInt(survivedTime / 60f);
                int secs = Mathf.FloorToInt(survivedTime % 60f);
                int reachedLevel = (playerLevel != null) ? playerLevel.CurrentLevel : 1;
                string recordBadge = isNewRecord ? "\n* NEW RECORD! *" : "";

                summaryText.text =
                    $"Time Survived: {mins:00}:{secs:00}\n" +
                    $"Enemies Slain: {killCount}\n" +
                    $"Level Reached: LV. {reachedLevel}" +
                    recordBadge;
            }
        }
    }

    private bool SaveBestRecords(float survivedTime, int kills)
    {
        bool newRecord = false;

        float bestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY, 0f);
        int bestKills = PlayerPrefs.GetInt(BEST_KILLS_KEY, 0);

        if (survivedTime > bestTime)
        {
            PlayerPrefs.SetFloat(BEST_TIME_KEY, survivedTime);
            newRecord = true;
        }

        if (kills > bestKills)
        {
            PlayerPrefs.SetInt(BEST_KILLS_KEY, kills);
            newRecord = true;
        }
        if (newRecord) PlayerPrefs.Save();
        return newRecord;
    }

    public void TogglePause()
    {
        if (State == GameState.Ended) return;
        if (upgradeManager != null && upgradeManager.IsPanelOpen) return;

        if (State == GameState.Playing)
        {
            State = GameState.Paused;
            if (pausePanel != null) { pausePanel.SetActive(true); }
            Time.timeScale = 0f;
            AudioManager.Instance?.PlayUIClick();
        }
        else if (State == GameState.Paused)
        {
            State = GameState.Playing;
            if (pausePanel != null) { pausePanel?.SetActive(false); }

            Time.timeScale = 1f;
            AudioManager.Instance?.PlayUIClick();
        }
    }
    private void HandlePlayerDied()
    {
        EndGame("Game Over",false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.RestoreMusicVolume();
        AudioManager.Instance?.PlayUIClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.RestoreMusicVolume();
        AudioManager.Instance?.PlayUIClick();
        SceneManager.LoadScene("MainMenu");
    }
}
