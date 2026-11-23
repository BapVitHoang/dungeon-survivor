using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class UpgradeSelectionTests
{
    private GameObject managerObj;
    private GameObject playerObj;
    private UpgradeManager upgradeManager;
    private AutoShooter shooter;
    private List<ScriptableObject> createdAssets;

    [SetUp]
    public void SetUp()
    {
        createdAssets = new List<ScriptableObject>();

        managerObj = new GameObject("TestUpgradeManager");
        upgradeManager = managerObj.AddComponent<UpgradeManager>();

        playerObj = new GameObject("TestPlayer");
        shooter = playerObj.AddComponent<AutoShooter>();
    }

    [TearDown]
    public void TearDown()
    {
        if (managerObj != null) Object.DestroyImmediate(managerObj);
        if (playerObj != null) Object.DestroyImmediate(playerObj);

        foreach (ScriptableObject asset in createdAssets)
        {
            if (asset != null) Object.DestroyImmediate(asset);
        } 
        createdAssets.Clear();
    }

    private StatUpgradeSO CreateStatUpgrade(StatType type, float value)
    {
        StatUpgradeSO up = ScriptableObject.CreateInstance<StatUpgradeSO>();
        up.statType = type;
        up.value = value;
        createdAssets.Add(up);
        return up;
    }

    private MultiShotUpgradeSO CreateMultiShotUpgrade(int extra)
    {
        MultiShotUpgradeSO up = ScriptableObject.CreateInstance<MultiShotUpgradeSO>();
        up.extraProjectiles = extra;
        createdAssets.Add(up);
        return up;
    }

    private void ConfigureManager(List<UpgradeSO> upgrades)
    {
        SerializedObject so = new SerializedObject(upgradeManager);
        so.FindProperty("player").objectReferenceValue = playerObj;

        SerializedProperty listProp = so.FindProperty("allUpgrades");
        listProp.arraySize = upgrades.Count;
        for (int i = 0; i < upgrades.Count; i++)
        {
            listProp.GetArrayElementAtIndex(i).objectReferenceValue = upgrades[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    [Test]
    public void GetRandomUpgrades_NeverReturnsDuplicates()
    {
        List<UpgradeSO> pool = new List<UpgradeSO>
        {
            CreateStatUpgrade(StatType.BulletDamage, 1f),
            CreateStatUpgrade(StatType.MoveSpeed, 0.5f),
            CreateStatUpgrade(StatType.MaxHealth, 1f),
            CreateStatUpgrade(StatType.MagnetRadius, 0.5f),
            CreateStatUpgrade(StatType.FireRate, 0.85f)
        };
        ConfigureManager(pool);

    
        for (int run = 0; run < 20; run++)
        {
            List<UpgradeSO> choices = upgradeManager.GetRandomUpgrades(3);

            Assert.AreEqual(3,choices.Count);

            HashSet<UpgradeSO> unique = new HashSet<UpgradeSO>(choices);
            Assert.AreEqual(3, unique.Count);
    
        } 
    }

    [Test]
    public void CanApply_ExcludesMaxedFireRateAndMultiShot()
    {
        StatUpgradeSO fireRateUp = CreateStatUpgrade(StatType.FireRate, 0.8f);
        MultiShotUpgradeSO multiShotUp = CreateMultiShotUpgrade(1);
        StatUpgradeSO damageUp = CreateStatUpgrade(StatType.BulletDamage, 1f);

        ConfigureManager(new List<UpgradeSO> { fireRateUp, multiShotUp, damageUp });

        shooter.UpgradeFireRate(0.1f);
        shooter.AddProjectiles(6);
        List<UpgradeSO> choices = upgradeManager.GetRandomUpgrades(3);

        Assert.IsFalse(fireRateUp.CanApply(playerObj));
        Assert.IsFalse(multiShotUp.CanApply(playerObj));
        Assert.IsTrue(damageUp.CanApply(playerObj));
        Assert.AreEqual(1, choices.Count);
        Assert.AreSame(damageUp,choices[0]);
    }

    [Test]
    public void GetRandomUpgrades_WhenPoolSmallerThanRequested_ReturnsAvailableCount()
    {
        ConfigureManager(new List<UpgradeSO>
        {
            CreateStatUpgrade(StatType.BulletDamage, 1f),
            CreateStatUpgrade(StatType.MoveSpeed, 0.5f)
        });

        List<UpgradeSO> choices = upgradeManager.GetRandomUpgrades(3);
        Assert.AreEqual(2, choices.Count);
    }
}