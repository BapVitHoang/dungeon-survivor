using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [SerializeField] private float survivalTime = 120f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text resultText;

    private float remainingTime;
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

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
       
        if (isGameOver)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <=0)
        {
            remainingTime = 0;
            WinGame();
        }

        UpdateTimerUI();
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

    private void WinGame()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("You Survived! You Win!");
        if (endPanel != null)
        {
            endPanel.SetActive(true);
            timerText.gameObject.SetActive(false);
            if (resultText != null)
            {
                resultText.text = "You Survived! You Win!";
            }
        }
    }

    private void HandlePlayerDied()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("You Died! Game Over!");
        if (endPanel != null)
        {
            endPanel.SetActive(true);
            timerText.gameObject.SetActive(false);
            if (resultText != null)
            {
                resultText.text = "You Died! Game Over!";
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
