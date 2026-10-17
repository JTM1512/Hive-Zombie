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
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // Make percentage for Slider. This is a value between 0 and 1.
        if (healthBar != null) 
        {
            healthBar.value = health / maxHealth;
        }
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0f) return;

        health = Mathf.Max(0f, health - amount);
        if (health > 0f) return;

        ZombieFollow follow = GetComponent<ZombieFollow>();
        if (follow == null || !follow.IsFollowing) return;

        if (Tracker.Instance != null)
        {
            Tracker.Instance.RemoveZombie();
        }
        Destroy(gameObject);
    }


}
