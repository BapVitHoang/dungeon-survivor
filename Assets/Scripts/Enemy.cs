using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action OnAnyEnemyDied;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private GameObject xpGemPrefab;
    [SerializeField] private AudioClip hitSFX;

    private int currentHealth;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    void Start()
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
    private void FixedUpdate()
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
