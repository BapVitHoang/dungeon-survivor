using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    
    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        lifetime = Mathf.Max(lifetime, 0.1f); // Ensure lifetime is positive
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }
            Destroy(gameObject);
        }
    }
}
