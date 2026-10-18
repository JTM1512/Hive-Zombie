using UnityEngine;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    float health;
    [SerializeField] Slider healthBar;

    // Hunger reads these. It does not touch the private number.
    public float currentHealth => health;
    public float maxHealthValue => maxHealth;
    void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = maxHealth;
        RefreshHealthBar();
    }

    void RefreshHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.wholeNumbers = false;
            healthBar.value = health;
        }
    }

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        RefreshHealthBar();
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0f) return;

        health = Mathf.Max(0f, health - amount);
        RefreshHealthBar();
        if (health > 0f) return;

        // The player has no follower script. This is their death.
        if (GetComponent<PlayerMovement>() != null)
        {
            if (GameOverMenu.Instance != null)
                GameOverMenu.Instance.Show();
            return;
        }

        ZombieFollow follow = GetComponent<ZombieFollow>();
        if (follow == null || !follow.IsFollowing) return;

        // ZombieFollow owns follower death: it detaches the follower, updates the
        // horde count, and keeps the object alive until its death animation ends.
    }

    public void SetHealth(float amount)
    {
        maxHealth = Mathf.Max(1f, amount);
        health = maxHealth;
        RefreshHealthBar();
    }



}
