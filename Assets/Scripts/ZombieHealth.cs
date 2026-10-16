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
    // Awake is called when the script instance is being loaded
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
        health = Mathf.Max(0, health - amount);
    }

    public void SetHealth(float amount)
    {
        // This method sets both the current health and the maximum health to the specified amount.
        maxHealth = amount;
        health = amount;
    }



}
