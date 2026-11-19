using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public event Action<int, int, int> OnLevelChange;
    public event Action OnLevelUp;

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int xpToNextLevel = 3;
    [SerializeField] private float magnetRadius = 2.5f;

    public float MagnetRadius => magnetRadius;
    public int CurrentLevel => currentLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        OnLevelChange?.Invoke(currentXP,xpToNextLevel,currentLevel);
    }
    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();     
        }
        OnLevelChange?.Invoke(currentXP, xpToNextLevel, currentLevel);
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = currentLevel * 3; // Adjust XP required for next level
        OnLevelUp?.Invoke();
        Debug.Log("Leveled up to level " + currentLevel + "! New XP to next level: " + xpToNextLevel);
    }

    public void UpgradeMagnetRadius(float amount)
    {
        magnetRadius += amount;
        Debug.Log("Magnet radius upgraded! New magnet radius: " + magnetRadius);
    }
}
