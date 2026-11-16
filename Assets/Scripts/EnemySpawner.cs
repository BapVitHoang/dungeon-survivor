using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.5f; // Minimum spawn interval   
    [SerializeField] private float spawnIntervalDecrease = 0.03f; // Amount to decrease spawn interval each time
    [SerializeField] private float spawnRadius = 10f;

    private Transform playerTransform;
    private float spawnTimer;

    void Start()
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
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease); // Decrease spawn interval but not below minimum
            SpawnEnemy();
        }
    }
    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos =  playerTransform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
        
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }
}
