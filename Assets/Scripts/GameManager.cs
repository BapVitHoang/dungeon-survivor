using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public const string BEST_TIME_KEY = "BestTime";
    public const string BEST_KILLS_KEY = "BestKills";

    [Header("Gameplay References")]
    [SerializeField] private float survivalTime = 120f;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerLevel playerLevel;

    [Header("HUD UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text killCountText;


    [Header("End & Pause Panels")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private GameObject pausePanel;

    private float remainingTime;
    private int killCount = 0;
    private bool isPaused = false;
    private bool isGameOver = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1f;
        remainingTime = survivalTime;

        if (playerHealth != null)
        {
            playerHealth.OnDied += HandlePlayerDied;
        }
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
    }
    // Update is called once per frame
    private void Update()
    {
       
        if (isGameOver)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (isPaused) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <=0)
        {
            remainingTime = 0;
            EndGame("YOU SURVIVED!");
        }

        UpdateTimerUI();
    }

    private void HandleEnemyDied()
    {
        killCount++;
        UpdateKillUI();
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

    private void EndGame(string titleMessage)
    {
        if (isGameOver) return;
        isGameOver = true;
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
            }

            if (summaryText != null)
            {
                int mins = Mathf.FloorToInt(survivedTime / 60f);
                int secs = Mathf.FloorToInt(survivedTime % 60f);
                int reachedLevel = (playerLevel != null) ? playerLevel.CurrentLevel : 1;
                string recordBadge = isNewRecord ? "\n★ NEW RECORD! ★" : "";

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
        if (isGameOver) return;

        if(!isPaused && Time.timeScale == 0f) return;

        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f: 1f;
    }
    private void HandlePlayerDied()
    {
        EndGame("Game Over");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.RestoreMusicVolume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.RestoreMusicVolume();
        SceneManager.LoadScene("MainMenu");
    }
}
