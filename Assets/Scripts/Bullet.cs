using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask obstacleMask;

    private Vector2 direction = Vector2.right;
    private int damage = 1;
    private float lifetimeTimer;
    private bool isReleased;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 newDirection, int bulletDamage)
    {
        isReleased = false;
        lifetimeTimer = Mathf.Max(lifetime,0.1f);
        direction = newDirection.normalized;
        damage = bulletDamage;
        rb.linearVelocity = direction * speed;
        transform.up = direction;
    }

    private void Update()
    {
        if(isReleased) return;
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0 )
        {
            ReleaseToPool();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReleased) return;
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage,direction);
                ReleaseToPool();
                return;
            }
            
        }
        else if ((1 << collision.gameObject.layer & obstacleMask.value) != 0 )
        {
            ReleaseToPool();
        }
    }

    private void ReleaseToPool()
    {
        if (isReleased) return ;
        isReleased = true;
        rb.linearVelocity = Vector2.zero;
        if (PoolManager.Instance !=  null )
        {
            PoolManager.Instance.ReleaseBullet(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
