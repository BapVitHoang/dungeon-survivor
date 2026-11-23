using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CombatPoolingTests
{
    private List<GameObject> spawnedObjects;
    private PoolManager poolManager;

    [SetUp]
    public void SetUp()
    {
        spawnedObjects = new List<GameObject>();
        Time.timeScale = 1f;
    }

    [TearDown]
    public void TearDown()
    {
        if (poolManager != null)
        {
            InvokePrivate(poolManager, "OnDestroy");
            poolManager = null;
        }

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
        } 
        spawnedObjects.Clear();

        Time.timeScale = 1f;
    }

    private GameObject CreateTrackedObject(string name)
    {
        GameObject obj = new GameObject(name);
        spawnedObjects.Add(obj);
        return obj;
    }

    private void SetupPoolManager()
    {
        GameObject bulletTemplate = CreateTrackedObject("BulletTemplate");
        bulletTemplate.SetActive(false);
        bulletTemplate.AddComponent<Rigidbody2D>();
        Bullet bulletComp = bulletTemplate.AddComponent<Bullet>();

        GameObject gemTemplate = CreateTrackedObject("GemTemplate");
        gemTemplate.SetActive(false);
        XPGem gemComp = gemTemplate.AddComponent<XPGem>();

        GameObject poolObj = CreateTrackedObject("TestPoolManager");
        poolManager = poolObj.AddComponent<PoolManager>();

        SerializedObject so = new SerializedObject(poolManager);
        so.FindProperty("bulletPrefab").objectReferenceValue = bulletComp;
        so.FindProperty("xpgemPrefab").objectReferenceValue = gemComp;
        so.ApplyModifiedPropertiesWithoutUndo();

        InvokePrivate(poolManager, "Awake");
    }

    private object InvokePrivate(object target, string methodName, params object[] args)
    {
        Type type = target.GetType();
        while (type != null)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                return method.Invoke(target, args);
            }
            type = type.BaseType;
        } 

        Assert.Fail($"Method '{methodName}' not found on type '{target.GetType().Name}'.");
        return null;
    }

    [Test]
    public void Bullet_DoubleCollisionSameFrame_ReleasesOnce()
    {
        SetupPoolManager();

        GameObject enemyObj = CreateTrackedObject("TestEnemy");
        enemyObj.tag = "Enemy";
        enemyObj.AddComponent<Rigidbody2D>();
        CircleCollider2D enemyCol = enemyObj.AddComponent<CircleCollider2D>();
        Enemy enemy = enemyObj.AddComponent<Enemy>();

        SerializedObject enemySo = new SerializedObject(enemy);
        enemySo.FindProperty("maxHealth").intValue = 5;
        enemySo.ApplyModifiedPropertiesWithoutUndo();

        InvokePrivate(enemy, "Awake");
        enemy.Initialize(EnemyType.Normal, null);

        int lastHp = 5;
        int damageEvents = 0;
        enemy.OnHealthChanged += (cur, max) =>
        {
            lastHp = cur;
            damageEvents++;
        };

        Bullet bullet = poolManager.GetBullet(Vector3.zero);
        InvokePrivate(bullet, "Awake");
        bullet.Initialize(Vector2.right, 1);
        InvokePrivate(bullet, "OnTriggerEnter2D", enemyCol);
        InvokePrivate(bullet, "OnTriggerEnter2D", enemyCol);

        Assert.AreEqual(4, lastHp);
        Assert.AreEqual(1, damageEvents);
        Assert.IsFalse(bullet.gameObject.activeSelf);
    }

    [Test]
    public void XPGem_DoubleTrigger_GrantsXpAndReleasesOnce()
    {
        // Arrange
        SetupPoolManager();

        GameObject playerObj = CreateTrackedObject("TestPlayer");
        CircleCollider2D playerCol = playerObj.AddComponent<CircleCollider2D>();
        PlayerLevel playerLevel = playerObj.AddComponent<PlayerLevel>();

        int lastXP = 0;
        int xpChangeEvents = 0;
        playerLevel.OnLevelChange += (xp, xpToNext, level) =>
        {
            lastXP = xp;
            xpChangeEvents++;
        };

        XPGem gem = poolManager.GetXPGem(Vector3.zero);
        gem.Initialize(playerObj.transform, playerLevel);

        InvokePrivate(gem, "OnTriggerEnter2D", playerCol);
        InvokePrivate(gem, "OnTriggerEnter2D", playerCol);

        Assert.AreEqual(1, lastXP);
        Assert.AreEqual(1, xpChangeEvents);
        Assert.IsFalse(gem.gameObject.activeSelf);
    }

    [Test]
    public void ChargerEnemy_HitFlashDuringTelegraph_PreservesTelegraphColor()
    {
   
        GameObject chargerObj = CreateTrackedObject("TestCharger");
        chargerObj.AddComponent<Rigidbody2D>();
        SpriteRenderer sr = chargerObj.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        ChargerEnemy charger = chargerObj.AddComponent<ChargerEnemy>();

        InvokePrivate(charger, "Awake");

        FieldInfo stateField = typeof(ChargerEnemy).GetField("currentState", BindingFlags.Instance | BindingFlags.NonPublic);
        object telegraphValue = Enum.Parse(stateField.FieldType, "Telegraph");
        stateField.SetValue(charger, telegraphValue);

        IEnumerator flash = (IEnumerator)InvokePrivate(charger, "HitFlash");
        flash.MoveNext();
        Assert.AreEqual(Color.red, sr.color);
        flash.MoveNext();
        Assert.AreEqual(Color.yellow, sr.color);
        charger.Initialize(EnemyType.Charger, null);
        Assert.AreEqual(Color.white, sr.color);
    }

    [Test]
    public void GameManager_EscWhileUpgradePanelOpen_DoesNotTogglePause()
    {
     
        GameObject pausePanelObj = CreateTrackedObject("PausePanel");
        pausePanelObj.SetActive(false);

        GameObject umObj = CreateTrackedObject("TestUpgradeManager");
        UpgradeManager upgradeManager = umObj.AddComponent<UpgradeManager>();

        GameObject gmObj = CreateTrackedObject("TestGameManager");
        GameManager gameManager = gmObj.AddComponent<GameManager>();

        SerializedObject gmSo = new SerializedObject(gameManager);
        gmSo.FindProperty("upgradeManager").objectReferenceValue = upgradeManager;
        gmSo.FindProperty("pausePanel").objectReferenceValue = pausePanelObj;
        gmSo.ApplyModifiedPropertiesWithoutUndo();

        PropertyInfo isPanelOpenProp = typeof(UpgradeManager).GetProperty("IsPanelOpen", BindingFlags.Instance | BindingFlags.Public);
        isPanelOpenProp.SetValue(upgradeManager, true);
        Time.timeScale = 0f;

        gameManager.TogglePause();
        Assert.AreEqual(GameState.Playing, gameManager.State);
        Assert.IsFalse(pausePanelObj.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);

        isPanelOpenProp.SetValue(upgradeManager, false);
        Time.timeScale = 1f;
        gameManager.TogglePause();
        Assert.AreEqual(GameState.Paused, gameManager.State);
        Assert.IsTrue(pausePanelObj.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);
    }
}