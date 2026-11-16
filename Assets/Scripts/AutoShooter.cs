using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float fireRange = 10f;
    [SerializeField] private AudioClip shootSFX;

    float fireTimer;

    // Update is called once per frame
    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            ShootNearestEnemy();
        }
    }

    private void ShootNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance?.PlaySFX(shootSFX);
        Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDirection(direction);
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float minDistance = fireRange;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }
        return nearestEnemy;
    }

    public void UpgradeFireRate()
    {
        fireInterval = Mathf.Max(0.15f, fireInterval * 0.8f); // Decrease fire interval but not below 0.1 seconds
        Debug.Log("Fire rate upgraded! New fire interval: " + fireInterval);

    }
}
