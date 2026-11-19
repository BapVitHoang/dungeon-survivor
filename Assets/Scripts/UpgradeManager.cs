using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
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

    private List<UpgradeSO> currentChoices = new List<UpgradeSO>();

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
        }

        if (playerLevel != null)
        {
            playerLevel.OnLevelChange += UpdateXPUI;
            playerLevel.OnLevelUp += ShowUpgradePanel;
            UpdateXPUI(0, 3, playerLevel.CurrentLevel);
        }

    }

    private void OnDestroy()
    {
        if (playerLevel != null)
        {
            playerLevel.OnLevelChange -= UpdateXPUI;
            playerLevel.OnLevelUp -= ShowUpgradePanel;
        }
    }

    private void UpdateXPUI( int currentXP, int xpToNext, int level)
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

    private void ShowUpgradePanel()
    {
        if (upgradePanel == null) return;

        Time.timeScale = 0f;
        upgradePanel.SetActive(true);
        currentChoices = GetRandomUpgrades(cardButtons.Length);

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
        ApplyUpgrade(currentChoices[index]);
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    public List<UpgradeSO> GetRandomUpgrades(int count)
    {
        List<UpgradeSO> pool = new List<UpgradeSO>(allUpgrades);
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
