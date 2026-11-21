using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float fireRange = 10f;
    [SerializeField] private AudioClip shootSFX;

    [Header("Targeting & LOS")]
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Weapon Stats")]
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 15f;

    private readonly Collider2D[] hitBuffer = new Collider2D[32];
    private ContactFilter2D enemyFilter;
    private float fireTimer;

    private void Awake()
    {
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(enemyLayerMask);
        enemyFilter.useTriggers = false;
    }
    private void Update()
    {
        fireTimer = Mathf.Min(fireTimer + Time.deltaTime, fireInterval);
        if (fireTimer >= fireInterval)
        {
            if (TryShootNearestEnemy())
            {
                fireTimer = 0f;
            }
        }
    }

/*    private void ShootNearestEnemy()
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

    }*/

    private bool TryShootNearestEnemy()
    {
        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return false;

        Vector2 baseDirection = (nearestEnemy.position - transform.position).normalized;
        AudioManager.Instance?.PlaySFX(shootSFX);

        float startAngle = -((projectileCount - 1) * spreadAngle) / 2f;
        for (int i = 0;i < projectileCount;i++)
        {
            float angle = startAngle + i * spreadAngle;
            Vector2 dir = Quaternion.Euler(0f,0f,angle) * baseDirection;

            Bullet bullet;
            if (PoolManager.Instance != null)
            {
                bullet = PoolManager.Instance.GetBullet(transform.position);
            }
            else
            {
                GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet = bulletObj.GetComponent<Bullet>();
            }

            if (bullet != null)
            {
                bullet.Initialize(dir, bulletDamage);
            }
        }
        return true;
    }
    private Transform FindNearestEnemy()
    {

        int hitCount = Physics2D.OverlapCircle(transform.position, fireRange, enemyFilter,hitBuffer);
        Transform nearestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            Transform candiate = hitBuffer[i].transform;
            float distanceSqr = (candiate.position - transform.position).sqrMagnitude;
            if (distanceSqr >= closestDistanceSqr) continue;
            if (Physics2D.Linecast(transform.position, candiate.position, obstacleMask).collider != null) continue;
            closestDistanceSqr = distanceSqr;
            nearestEnemy = candiate;
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

    public bool CanUpgradeFireRate() => fireInterval > 0.15f;

    public bool CanAddProjectiles() => projectileCount < 7;
}


