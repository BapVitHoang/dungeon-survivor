using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action OnAnyEnemyDied;

    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int maxHealth = 2;
    [SerializeField] protected int damage = 1;
    [SerializeField] private GameObject xpGemPrefab;
    [SerializeField] private AudioClip hitSFX;

    public int Damage => damage;

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected Transform playerTransform;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    private Coroutine flashCoroutine;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    protected virtual void FixedUpdate()
    {
        if (playerTransform != null)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            if (spriteRenderer != null)
            {
                if (direction.x < -0.01f)
                {
                    spriteRenderer.flipX = true; // Face left
                }
                else if (direction.x > 0.01f)
                {
                    spriteRenderer.flipX = false; // Face right
                }
            }
            rb.linearVelocity = direction * moveSpeed;

        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        AudioManager.Instance?.PlaySFX(hitSFX);
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(HitFlash());
        }
    }

    private void Die()
    {
        // Spawn XP gem at the enemy's position
        if (xpGemPrefab != null)
        {
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
        }

  
        OnAnyEnemyDied?.Invoke();

        Destroy(gameObject);
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f); 
            spriteRenderer.color = originalColor; 
        }
        flashCoroutine = null;
    }
}
