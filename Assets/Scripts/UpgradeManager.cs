using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public bool IsPanelOpen { get; private set; }

    [Header("References")]
    [SerializeField] private List<UpgradeSO> allUpgrades = new List<UpgradeSO>();
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerLevel playerLevel;

    [Header("Top HUD (XP & Level)")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text levelText;

    [Header("Level-Up Panel UI (3 Cards)")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] cardButtons;
    [SerializeField] private TMP_Text[] cardTitleTexts;
    [SerializeField] private TMP_Text[] cardDescTexts;
    [SerializeField] private Image[] cardIcons;

    [Header("Audio")]
    [SerializeField] private AudioClip levelUpSFX;

    private List<UpgradeSO> currentChoices = new List<UpgradeSO>();
    private int pendingLevelUps = 0;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerLevel == null && player != null)
        {
            playerLevel = player.GetComponent<PlayerLevel>();
        }

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            IsPanelOpen = false;
        }

        if (playerLevel != null)
        {
            playerLevel.OnLevelChange += UpdateXPUI;
            playerLevel.OnLevelUp += HandleLevelUp;
            UpdateXPUI(0, 3, playerLevel.CurrentLevel);
        }

    }

    private void OnDestroy()
    {
        if (playerLevel != null)
        {
            playerLevel.OnLevelChange -= UpdateXPUI;
            playerLevel.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Update()
    {
        if (!IsPanelOpen || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame && currentChoices.Count >= 1)
        {
            SelectCard(0);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame && currentChoices.Count >= 2)
        {
            SelectCard(1);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame && currentChoices.Count >= 3)
        {
            SelectCard(2);
        }

    }

    private void UpdateXPUI(int currentXP, int xpToNext, int level)
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNext;
            xpSlider.value = currentXP;
        }

        if (levelText != null)
        {
            levelText.text = "LV." + level;
        }
    }

    private void HandleLevelUp()
    {
        pendingLevelUps++;
        if (!IsPanelOpen)
        {
            ShowNextUpgradePanel();
        }
    }
    private void ShowNextUpgradePanel()
    {
        if (upgradePanel == null)
        {
            pendingLevelUps = 0;
            return;
        }

        pendingLevelUps = Mathf.Max(0, pendingLevelUps-1);
        currentChoices = GetRandomUpgrades(cardButtons.Length);

        if (currentChoices.Count ==0)
        {
            upgradePanel.SetActive(false);
            IsPanelOpen = false;
            pendingLevelUps = 0;
            Time.timeScale = 1f;
            return;
        }
        IsPanelOpen = true;
        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
        AudioManager.Instance?.PlaySFX(levelUpSFX, 0.7f);

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                UpgradeSO card = currentChoices[i];

                if (i < cardTitleTexts.Length && cardTitleTexts[i] != null)
                    cardTitleTexts[i].text = card.upgradeName;
                if (i < cardDescTexts.Length && cardDescTexts[i] != null)
                    cardDescTexts[i].text = card.description;
                if (i < cardIcons.Length && cardIcons[i] != null)
                    cardIcons[i].sprite = card.icon;

                int index = i;
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => SelectCard(index));
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectCard(int index)
    {
        if (!IsPanelOpen || index < 0 || index >= currentChoices.Count)
        {
            return;
        }
        AudioManager.Instance?.PlayUIClick();
        ApplyUpgrade(currentChoices[index]);
        if (pendingLevelUps > 0)
        {
            ShowNextUpgradePanel();
        }
        else
        {
            upgradePanel.SetActive(false);
            IsPanelOpen = false;
            Time.timeScale = 1f;
        }
        
    }
    public List<UpgradeSO> GetRandomUpgrades(int count)
    {
        List<UpgradeSO> pool = new List<UpgradeSO>();
        for (int i =0; i < allUpgrades.Count; i++)
        {
            UpgradeSO upgrade = allUpgrades[i];
            if (upgrade != null && (player == null || upgrade.CanApply(player)))
            {
                pool.Add(upgrade);
            }
        }
        List<UpgradeSO> result = new List<UpgradeSO>();

        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0)
            {
                break;
            }

            int randomIndex = Random.Range(0, pool.Count);
            UpgradeSO selectedUpgrade = pool[randomIndex];
            result.Add(selectedUpgrade);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }

    public void ApplyUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade != null && player != null)
        {
            selectedUpgrade.Apply(player);
        }
    }
}
