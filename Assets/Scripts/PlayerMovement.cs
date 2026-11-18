using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private InputAction moveAction;
    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        // Flip the sprite based on movement direction
        if (spriteRenderer != null)
        {
            if (moveInput.x < -0.01f)
            {
                spriteRenderer.flipX = true; // Face left
            }
            else if (moveInput.x > 0.01f)
            {
                spriteRenderer.flipX = false; // Face right
            }
        }
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate. Put physics code here.
    private void FixedUpdate()
    {
        Vector2 movement = moveInput.normalized * moveSpeed;

        rb.linearVelocity = movement;
    }

    public void UpgradeMoveSpeed(float amount)
    {
        moveSpeed += amount;
        Debug.Log($"Move speed upgraded by {amount}. New move speed: {moveSpeed}");
    }


}
