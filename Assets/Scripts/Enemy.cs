using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action OnAnyEnemyDied;
    public static event Action OnEliteEnemyDied;
    public event Action<int, int> OnHealthChanged;

    private static readonly float[] WhiskerAngles = { 45f, -45f, 80f, -80f };

    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int maxHealth = 2;
    [SerializeField] protected int damage = 1;
    [SerializeField] private GameObject xpGemPrefab;
    [SerializeField] private AudioClip hitSFX;

    [Header("Obstacle Avoidance & Knockback")]
    [SerializeField] protected LayerMask obstacleMask;
    [SerializeField] protected float avoidCheckDistance = 1.1f;
    [SerializeField] protected float knockbackForce = 4f;
    [SerializeField] protected float knockbackDuration = 0.08f;
    [Range(0f, 1f)]
    [SerializeField] protected float knockbackResistance = 0f;
    public int Damage => damage;

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected Transform playerTransform;
    protected PlayerLevel playerLevel;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected EnemyType enemyType = EnemyType.Normal;
    protected bool isDead;
    protected float avoidSign = 1f;

    private Coroutine flashCoroutine;
    private float knockbackTimer;
    private Vector2 knockbackVelocity;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null )
        {
            originalColor = spriteRenderer.color;
        }
    }

    public virtual void Initialize(EnemyType type, Transform playerTarget, PlayerLevel levelRef = null)
    {
        enemyType = type;
        playerTransform = playerTarget;
        playerLevel = levelRef;

        if ( playerTransform != null && playerLevel == null)
        {
            playerLevel = playerTransform.GetComponent<PlayerLevel>();        
        }
        isDead = false;
        currentHealth = maxHealth;
        knockbackTimer = 0f;
        knockbackVelocity = Vector2.zero;
        avoidSign = (UnityEngine.Random.value < 0.5f) ? 1f : -1f;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;         
        }
        StopAllCoroutines();
        flashCoroutine = null;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    protected virtual void Start()
    {
        if (playerTransform == null)
        {
            currentHealth = maxHealth;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerLevel = playerObj.GetComponent<PlayerLevel>();
            }
        }
        
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    protected virtual void FixedUpdate()
    {
        if (isDead || playerTransform == null)
        {
            return;
        }
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = knockbackVelocity;
            return;
        }
        Vector2 desiredDir = (playerTransform.position - transform.position).normalized;
        Vector2 moveDir = GetSteeredDirection(desiredDir);

        if (spriteRenderer != null)
        {
            if (desiredDir.x < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
            else if (desiredDir.x > 0.01f)
            {
                spriteRenderer.flipX = false;

            }

        }
        rb.linearVelocity = moveDir* moveSpeed;

        
    }
   
    protected Vector2 GetSteeredDirection(Vector2 desiredDir)
    {
        RaycastHit2D forwardHit = Physics2D.Raycast(transform.position, desiredDir, avoidCheckDistance,obstacleMask);
        if ( forwardHit.collider == null )
        {
            return desiredDir;
        }
        for (int i = 0; i < WhiskerAngles.Length; i++)
        {
            float angle = WhiskerAngles[i] * avoidSign;
            Vector2 testDir = Quaternion.Euler(0f,0f,angle) * desiredDir;
            RaycastHit2D whiskerHit = Physics2D.Raycast(transform.position, testDir, avoidCheckDistance, obstacleMask);
            if ( whiskerHit.collider == null )
            {
                return testDir.normalized;
            }
        }

        return (Vector2.Perpendicular(forwardHit.normal) * avoidSign).normalized;
    }

    protected virtual Color GetBaseColor()
    {
        return originalColor;
    }
    protected virtual bool CanTakeKnockback()
    {
        return true;
    }
    public void TakeDamage(int damage, Vector2 hitDirection = default)
    {
        if (isDead)
        {
            return;
        }
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth,maxHealth);
        AudioManager.Instance?.PlaySFX(hitSFX);
        if (PoolManager.Instance != null)
        {
            float offsetX = UnityEngine.Random.Range(-0.2f, 0.2f);
            Vector3 popupPos = transform.position + new Vector3(offsetX,0.55f,0f);
            Color popupColor = (damage >=2) ? Palette.AccentGold : Palette.TextPrimary;
            PoolManager.Instance.SpawnDamagePopup(popupPos,damage,popupColor);
        }
        if (CanTakeKnockback() && hitDirection.sqrMagnitude > 0.001f)
        {
            float effectiveForce = knockbackForce * (1f - knockbackResistance);
            knockbackVelocity = hitDirection.normalized * effectiveForce;
            knockbackTimer = knockbackDuration;
        }

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
        if (isDead)
        {
            return;
        }
        else { isDead = true; }

        StopAllCoroutines();
        flashCoroutine = null;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        // Spawn XP gem at the enemy's position
        if (PoolManager.Instance != null)
        {
            XPGem gem = PoolManager.Instance.GetXPGem(transform.position);
            gem.Initialize(playerTransform,playerLevel);
        }
        else if (xpGemPrefab != null)
        {
            Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
        }
        if (PoolManager.Instance !=null)
        {
            Color burstColor = (enemyType == EnemyType.Elite) ? Palette.AccentGold : Palette.HealthCrimson;
            PoolManager.Instance.SpawnDeathVFX(transform.position, burstColor);
        }
  
        OnAnyEnemyDied?.Invoke();
        if (enemyType == EnemyType.Elite)
        {
            OnEliteEnemyDied?.Invoke();
        }
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReleaseEnemy(enemyType, this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f); 
            spriteRenderer.color = GetBaseColor(); 
        }
        flashCoroutine = null;
    }
}
