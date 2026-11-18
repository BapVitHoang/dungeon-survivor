using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float fireRange = 10f;
    [SerializeField] private AudioClip shootSFX;

    [Header("Weapon Stats")]
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 15f;

    private float fireTimer;

    private void Update()
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
        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 baseDirection = (nearestEnemy.position - transform.position).normalized;
        AudioManager.Instance?.PlaySFX(shootSFX);

        float startAngle = -((projectileCount - 1) * spreadAngle) / 2f;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + i * spreadAngle;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDirection;
            GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(dir, bulletDamage);
            }
        }

    }

    private Transform FindNearestEnemy()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fireRange);
        Transform nearestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distanceSqr = (hit.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    nearestEnemy = hit.transform;
                }
            }
        }
        return nearestEnemy;
    }


    public void UpgradeFireRate(float multiplier = 0.8f)
    {
        fireInterval = Mathf.Max(0.15f, fireInterval * multiplier);
        Debug.Log("Fire rate upgraded! New fire interval: " + fireInterval);
    }

    public void UpgradeDamage(int amount)
    {
        bulletDamage = Mathf.Max(1, bulletDamage + amount);
    }

    public void AddProjectiles(int amount)
    {
        projectileCount = Mathf.Min(7, projectileCount + amount);
    }
}


