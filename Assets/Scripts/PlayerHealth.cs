using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private AudioClip damageSFX;

    private int currentHealth;
    private float cooldownTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
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
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            int dmg = enemy != null ? enemy.Damage : 1; // Default damage to 1 if Enemy component is not found
            TakeDamage(dmg);
            cooldownTimer = damageCooldown;
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        AudioManager.Instance?.PlaySFX(damageSFX);
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("GAME OVER");
        gameObject.SetActive(false);
        OnDied?.Invoke();
    }

    public void UpgradeMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    public event Action OnDied;
}
