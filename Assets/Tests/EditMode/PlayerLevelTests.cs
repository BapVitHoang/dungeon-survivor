using UnityEngine;
using NUnit.Framework;

public class PlayerLevelTests
{
    private GameObject testObj;
    private PlayerLevel playerLevel;

    [SetUp]
    public void SetUp()
    {
        testObj = new GameObject("TestPlayer");
        playerLevel = testObj.AddComponent<PlayerLevel>();
    }

    [TearDown]
    public void TearDown()
    {
        if (testObj != null)
        {
            Object.DestroyImmediate(testObj);
        }
    }

    [Test]
    public void AddXP_ExactThreshold_AdvancesOneLevel()
    {
        int levelUpCount = 0;
        int lastXP = -1;
        int lastXpToNext = -1;
        int lastLevel = -1;

        playerLevel.OnLevelUp += () => levelUpCount++;
        playerLevel.OnLevelChange += (xp, xpToNext, level) =>
        {
            lastXP = xp;
            lastXpToNext = xpToNext;
            lastLevel = level;
        };

        playerLevel.AddXP(3);

        Assert.AreEqual(2, playerLevel.CurrentLevel);
        Assert.AreEqual(1, levelUpCount);
        Assert.AreEqual(0, lastXP);
        Assert.AreEqual(6, lastXpToNext);
        Assert.AreEqual(2, lastLevel);
    }

    [Test]
    public void AddXP_LargeBurst_RollsOverAndFiresMultipleLevelUps()
    {
        int levelUpCount = 0;
        int lastXP = -1;
        int lastXpToNext = -1;
        int lastLevel = -1;
        playerLevel.OnLevelUp += () => levelUpCount++;
        playerLevel.OnLevelChange += (xp, xpToNext, level) =>
        {
            lastXP = xp;
            lastXpToNext = xpToNext;
            lastLevel = level;
        };
        playerLevel.AddXP(11);

        Assert.AreEqual(3, playerLevel.CurrentLevel);
        Assert.AreEqual(2, levelUpCount);
        Assert.AreEqual(2, lastXP);
        Assert.AreEqual(9, lastXpToNext);
        Assert.AreEqual(3, lastLevel);
    }

    [Test]
    public void AddXP_ZeroOrNegative_DoesNotAdvanceOrFireEvents()
    {
     
        int levelUpCount = 0;
        int levelChangeCount = 0;
        playerLevel.OnLevelUp += () => levelUpCount++;
        playerLevel.OnLevelChange += (_, __, ___) => levelChangeCount++;
        playerLevel.AddXP(0);
        playerLevel.AddXP(-5);

        Assert.AreEqual(1, playerLevel.CurrentLevel);
        Assert.AreEqual(0, levelUpCount);
        Assert.AreEqual(0, levelChangeCount);
    }
}
