using UnityEngine;

public class ChargerEnemy : Enemy
{
    private enum State
    {
        Chase,
        Telegraph,
        Dash
    }

    [Header("Charger Enemy Settings")]

    [SerializeField] private float dashRange = 5f;
    [SerializeField] private float telegraphDuration = 0.4f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashSpeedMultiplier = 3f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private Color telegraphColor = Color.yellow;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private Transform telegraphIndicator;

    private State currentState = State.Chase;
    private float stateTimer;
    private float cooldownTimer = 1f;
    private Vector2 dashDirection;

    public override void Initialize(EnemyType type, Transform playerTarget, PlayerLevel levelRef = null)
    {
        base.Initialize(type, playerTarget, levelRef);
        currentState = State.Chase;
        stateTimer = 0f;
        cooldownTimer = 1f;
        dashDirection = Vector2.zero;
        if (telegraphIndicator != null)
        {
            telegraphIndicator.gameObject.SetActive(false);
        }
    }
    protected override Color GetBaseColor()
    {
        if (currentState == State.Telegraph) return telegraphColor;
        else return originalColor;

    }

    protected override bool CanTakeKnockback()
    {
        return currentState != State.Dash;
    }

    private bool HasClearDashPath()
    {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, playerTransform.position, obstacleMask);
        return hit.collider == null;
    }
    protected override void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;

        switch (currentState)
        {
            case State.Chase:
                base.FixedUpdate();

                cooldownTimer -= Time.fixedDeltaTime;

                float distanceSqr = (playerTransform.position - transform.position).sqrMagnitude;


                if (distanceSqr <= dashRange * dashRange && cooldownTimer <= 0 && HasClearDashPath())
                {
                    currentState = State.Telegraph;
                    stateTimer = telegraphDuration;

                    dashDirection = (playerTransform.position - transform.position).normalized;

                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = telegraphColor;
                    }
                    if (telegraphIndicator != null)
                    {
                        telegraphIndicator.gameObject.SetActive(true);
                        telegraphIndicator.right = dashDirection;
                    }
                }
                break;

            case State.Telegraph:
                rb.linearVelocity = Vector2.zero;
                stateTimer -= Time.fixedDeltaTime;
                if (stateTimer <= 0)
                {
                    currentState = State.Dash;
                    stateTimer = dashDuration;
                    if (telegraphIndicator != null)
                    {
                        telegraphIndicator.gameObject.SetActive(false);
                    }
                    AudioManager.Instance?.PlaySFX(dashSFX, 0.5f);
                }

                break;

            case State.Dash:
                rb.linearVelocity = dashDirection * (moveSpeed * dashSpeedMultiplier);
                stateTimer -= Time.fixedDeltaTime;
                if (stateTimer <= 0)
                {
                    currentState = State.Chase;
                    cooldownTimer = dashCooldown;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = originalColor;
                    }
                }
                break;
        }

    }

}
