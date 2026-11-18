using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject normalEnemyPrefab;
    [SerializeField] private GameObject chargerEnemyPrefab;
    [SerializeField] private GameObject eliteEnemyPrefab;


    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.5f; // Minimum spawn interval   
    [SerializeField] private float spawnIntervalDecrease = 0.03f; // Amount to decrease spawn interval each time
    [SerializeField] private float spawnRadius = 10f;

    [Header("Wave Pacing (Seconds)")]
    [SerializeField] private float phase2StartTime = 30f;
    [SerializeField] private float phase3StartTime = 75f;
    [SerializeField] private float eliteInterval = 25f;

    private Transform playerTransform;
    private float spawnTimer;
    private float elapsedTime;
    private float eliteTimer;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
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
        int count = (elapsedTime >= phase3StartTime) ? 2 : 1;
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = ChooseEnemyPrefab();
            if (prefab != null)
            {
                Instantiate(prefab, GetRandomSpawnPosition(), Quaternion.identity);
            }

        }
    }

    private GameObject ChooseEnemyPrefab()
    {
        if (elapsedTime < phase2StartTime)
        {
            return normalEnemyPrefab;
        }
        if (elapsedTime > phase3StartTime)
        {
            return (Random.value < 0.35f) ? chargerEnemyPrefab : normalEnemyPrefab;
        }
        return (Random.value < 0.5f) ? chargerEnemyPrefab : normalEnemyPrefab;
    }

    private void SpawnElite()
    {
        if (eliteEnemyPrefab == null) return;
        Instantiate(eliteEnemyPrefab, GetRandomSpawnPosition(),Quaternion.identity);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        return playerTransform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

    }
}
