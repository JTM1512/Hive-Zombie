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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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


}
