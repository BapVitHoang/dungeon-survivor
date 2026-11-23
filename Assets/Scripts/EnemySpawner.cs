using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArenaGenerator arenaGenerator;

    [Header("Spawn Limits & Radius")]
    [SerializeField] private int maxActiveEnemies = 55;
    [SerializeField] private float minSpawnRadius = 10.5f;
    [SerializeField] private float maxSpawnRadius = 13.5f;
    [SerializeField] private float spawnClearance = 0.6f;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.5f; // Minimum spawn interval   
    [SerializeField] private float spawnIntervalDecrease = 0.03f; // Amount to decrease spawn interval each time

    [Header("Wave Pacing (Seconds)")]
    [SerializeField] private float phase2StartTime = 30f;
    [SerializeField] private float phase3StartTime = 75f;
    [SerializeField] private float eliteInterval = 25f;

    private Transform playerTransform;
    private PlayerLevel playerLevel;
    private int activeEnemyCount;
    private float spawnTimer;
    private float elapsedTime;
    private float eliteTimer;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerLevel = playerObj.GetComponent<PlayerLevel>();
        }
        Enemy.OnAnyEnemyDied += HandleEnemyDied;
    }

    private void OnDestroy()
    {
        Enemy.OnAnyEnemyDied -= HandleEnemyDied;
    }
    private void HandleEnemyDied()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);

    }
    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease); // Decrease spawn interval but not below minimum
            SpawnWave();
        }
        if(elapsedTime > phase2StartTime)
        {
            eliteTimer += Time.deltaTime;
            if (eliteTimer >= eliteInterval)
            {
                eliteTimer = 0f;
                SpawnElite();
            }
        }
    }
    private void SpawnWave()
    {
        if (PoolManager.Instance == null)
        {
            return;
        }
        int count = (elapsedTime >= phase3StartTime) ? 2 : 1;
        for (int i = 0; i < count; i++)
        {
            if (activeEnemyCount >= maxActiveEnemies)
            {
                return;
            }
            EnemyType type = ChooseEnemyType();
            Vector3 spawnPos = GetValidSpawnPosition();
            Enemy enemy = PoolManager.Instance.GetEnemy(type, spawnPos);
            enemy.Initialize(type,playerTransform,playerLevel);
            activeEnemyCount++;
            

        }
    }

    private EnemyType ChooseEnemyType()
    {
       if (elapsedTime < phase2StartTime)
        {
            return EnemyType.Normal;
        }
       if (elapsedTime < phase3StartTime)
        {
            return (Random.value < 0.35f) ? EnemyType.Charger : EnemyType.Normal;
        }

        return (Random.value < 0.5f) ? EnemyType.Charger : EnemyType.Normal;
    }

    private void SpawnElite()
    {
        if (PoolManager.Instance ==null || activeEnemyCount >= maxActiveEnemies +3)
        {
            return;
        }
        Vector3 spawnPos = GetValidSpawnPosition();
        Enemy elite = PoolManager.Instance.GetEnemy(EnemyType.Elite, spawnPos);
        elite.Initialize(EnemyType.Elite, playerTransform,playerLevel);
        activeEnemyCount++;
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < 12; attempt++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir == Vector2.zero)
            {
                dir = Vector2.right;
            }
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 candidate = playerTransform.position + new Vector3(dir.x * radius, dir.y * radius, 0f);
            if (arenaGenerator == null || arenaGenerator.IsPositionWalkable(candidate,spawnClearance))
            {
                return candidate;
            }
            
        }
        if (arenaGenerator != null)
        {
            Bounds bounds = arenaGenerator.PlayableBounds;
            for (int attempt = 0; attempt < 18; attempt++)
            {
                float x = Random.Range(bounds.min.x + 1.5f, bounds.max.x - 1.5f);
                float y = Random.Range(bounds.min.y + 1.5f, bounds.max.y - 1.5f);
                Vector3 fallback = new Vector3(x, y, 0f);
                float distToPlayerSqr = (fallback - playerTransform.position).sqrMagnitude;
                if (arenaGenerator.IsPositionWalkable(fallback, spawnClearance) && distToPlayerSqr >= 25f)
                {
                    return fallback;
                }
                
            }
        }
        return Vector3.zero;
    }
}
