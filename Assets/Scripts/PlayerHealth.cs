using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public event Action OnDied;
    public event Action<int,int> OnHealthChanged;

    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private TMP_Text hpValueText;

    private int currentHealth;
    private float cooldownTimer;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        UpdateHealthUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && cooldownTimer <= 0)
        {
            cooldownTimer = damageCooldown;
            int dmg = 1;
            if (collision.gameObject.TryGetComponent(out Enemy enemy))
            {
                dmg = enemy.Damage;
            }
            TakeDamage(dmg);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        AudioManager.Instance?.PlaySFX(damageSFX);
        CameraFollow.Instance?.TriggerShake(0.18f, 0.22f);
        UpdateHealthUI();
        Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
 
            }
            blinkCoroutine = StartCoroutine(InvincibilityBlink());
        }
    }
    private IEnumerator InvincibilityBlink()
    {
        bool isFaded = false;
        while (cooldownTimer >0f)
        {
            isFaded = !isFaded;
            SetSpriteAlpha(isFaded ? 0.35f : 1f);
            yield return new WaitForSeconds(0.1f);
           
        }
        SetSpriteAlpha(1f);
        blinkCoroutine = null;
    }
    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer == null)
        {
            return;
        }
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
    private void Die()
    {
        SetSpriteAlpha(1f);
        Debug.Log("GAME OVER");
        gameObject.SetActive(false);
        OnDied?.Invoke();
    }
    private void OnDisable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        SetSpriteAlpha(1f);
    }
    public void UpgradeMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateHealthUI();   
    }
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (hpValueText != null)
        {
            hpValueText.text = $"{currentHealth}/{maxHealth}";
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
