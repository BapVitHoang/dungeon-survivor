using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 3;
    [SerializeField] private float magnetRadius = 2.5f;

    public float MagnetRadius => magnetRadius;

    private AutoShooter autoShooter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        autoShooter = GetComponent<AutoShooter>();
    }
    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0;
        xpToNextLevel = currentLevel * 3; // Adjust XP required for next level
        if (autoShooter != null)
        {
            autoShooter.UpgradeFireRate(); // Upgrade fire rate when leveling up
        }
        Debug.Log("Leveled up to level " + currentLevel + "! New XP to next level: " + xpToNextLevel);
    }

    public void UpgradeMagnetRadius(float amount)
    {
        magnetRadius += amount;
        Debug.Log("Magnet radius upgraded! New magnet radius: " + magnetRadius);
    }
}
