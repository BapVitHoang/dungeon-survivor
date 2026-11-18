using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction = Vector2.right;
    private int damage = 1;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 newDirection, int bulletDamage)
    {
        direction = newDirection.normalized;
        damage = bulletDamage;
        rb.linearVelocity = direction * speed;
        transform.up = direction;
        Destroy(gameObject, Mathf.Max(lifetime, 0.1f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {

                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
