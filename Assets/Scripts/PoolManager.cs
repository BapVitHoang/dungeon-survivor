using UnityEngine;
using UnityEngine.Pool;

public enum EnemyType
{
    Normal,
    Charger,
    Elite
}
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Enemy normalEnemyPrefab;
    [SerializeField] private Enemy chargerEnemyPrefab;
    [SerializeField] private Enemy eliteEnemyPrefab;
    [SerializeField] private XPGem xpgemPrefab;
    [SerializeField] private DeathVFX deathVFXPrefab;
    [SerializeField] private DamagePopup damagePopupPrefab;

    private ObjectPool<Bullet> bulletPool;
    private ObjectPool<Enemy> normalEnemyPool;
    private ObjectPool<Enemy> chargerEnemyPool;
    private ObjectPool<Enemy> eliteEnemyPool;
    private ObjectPool<XPGem> xpGemPool;
    private ObjectPool<DeathVFX> deathVFXPool;
    private ObjectPool<DamagePopup> damagePopupPool;

    private void Awake()
    {
        Instance = this;
        bulletPool = CreatePool(bulletPrefab, 30, 120);
        normalEnemyPool = CreatePool(normalEnemyPrefab, 20, 60);
        chargerEnemyPool = CreatePool(chargerEnemyPrefab, 10, 35);
        eliteEnemyPool = CreatePool(eliteEnemyPrefab, 3, 10);
        xpGemPool = CreatePool(xpgemPrefab, 30, 150);
        if(deathVFXPrefab!= null)
        {
            deathVFXPool = CreatePool(deathVFXPrefab,15,60);
        }
        if(damagePopupPrefab!= null)
        {
            damagePopupPool = CreatePool(damagePopupPrefab,20,60);
        }

    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private ObjectPool<T> CreatePool<T>(T prefab, int defaultCapacity, int maxSize) where T :Component
    {
        return new ObjectPool<T>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => { if (obj != null) Destroy(obj.gameObject); },
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
            );
    }

    public Bullet GetBullet(Vector3 position)
    {
        Bullet b = bulletPool.Get();
        b.transform.position = position;
        return b;
    }

    public void ReleaseBullet(Bullet bullet)
    {
        bulletPool.Release(bullet);
    }

    public Enemy GetEnemy(EnemyType type, Vector3 position)
    {
        Enemy enemy = GetEnemyPool(type).Get();
        enemy.transform.position = position;
        return enemy;

    }

    public void ReleaseEnemy(EnemyType type, Enemy enemy)
    {
        GetEnemyPool(type).Release(enemy);
    }

    private ObjectPool<Enemy> GetEnemyPool(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Charger: return chargerEnemyPool;
            case EnemyType.Elite: return eliteEnemyPool;
            default: return normalEnemyPool;
        }
    }

    public XPGem GetXPGem(Vector3 position)
    {
        XPGem gem = xpGemPool.Get();
        gem.transform.position = position;
        return gem;
    }

    public void ReleaseXPGem(XPGem gem)
    {
        xpGemPool.Release(gem);
    }

    public void SpawnDeathVFX(Vector3 position, Color tint)
    {
        if (deathVFXPool == null) return;

        DeathVFX vfx = deathVFXPool.Get();
        vfx.transform.position = position;
        vfx.Play(tint, ReleaseDeathVFX);

    }

    private void ReleaseDeathVFX(DeathVFX vfx)
    {
        deathVFXPool.Release(vfx);
    }

    public void SpawnDamagePopup(Vector3 position, int damageAmount, Color color)
    {
        if (damagePopupPool == null) return;
        DamagePopup popup = damagePopupPool.Get();
        popup.transform.position = position;
        popup.Play(damageAmount,color,ReleaseDamagePopup);
    }

    private void ReleaseDamagePopup(DamagePopup popup)
    {
        damagePopupPool.Release(popup);
    }
}
