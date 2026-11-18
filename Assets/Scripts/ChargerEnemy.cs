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

    private State currentState = State.Chase;
    private float stateTimer;
    private float cooldownTimer =1f;
    private Vector2 dashDirection;

    protected override void FixedUpdate()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case State.Chase:
                base.FixedUpdate();

                cooldownTimer -= Time.fixedDeltaTime;

                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

                if (distanceToPlayer <= dashRange && cooldownTimer <=0)
                {
                    currentState = State.Telegraph;
                    stateTimer = telegraphDuration;

                    dashDirection = (playerTransform.position - transform.position).normalized;

                    if (spriteRenderer)
                    {
                        spriteRenderer.color = telegraphColor;
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
                }
                break;

                case State.Dash:
                rb.linearVelocity = dashDirection * (moveSpeed * dashSpeedMultiplier);
                stateTimer -= Time.fixedDeltaTime;
                if (stateTimer <=0)
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
