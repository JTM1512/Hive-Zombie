using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ZombieHunger : MonoBehaviour
{
    [SerializeField] private float maxHunger = 100;
    [SerializeField] private float hungerDecreaseRate = 1f; // Hunger decrease per second
    [SerializeField] private float starveDamagePerSecond = 5f; // Damage per second when starving
    [SerializeField] private float testFoodValue = 25f; // Amount of hunger restored by test food
    [SerializeField] private float testHeal = 20f; // Amount of health restored by full hunger bar
    [SerializeField] private Slider hungerBar; // UI Slider to display hunger

    float hunger;
    ZombieHealth health; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hunger = maxHunger;
        health = GetComponent<ZombieHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        // Hunger decreases over time
        hunger -= hungerDecreaseRate * Time.deltaTime;
        if (hunger < 0)
        {
            hunger = 0;
        }
        
        if (hunger <=0 )
        {
            // If hunger is zero, take damage over time
            health.TakeDamage(starveDamagePerSecond * Time.deltaTime);
        }

        /*if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Eat(testFoodValue, testHeal);
        }*/

        if (hungerBar != null)
        {
            hungerBar.value = hunger / maxHunger;
        }
    }

    public void Eat(float foodValue, float healValue)
    {
        // Heal when eating food if health is not full
        if (hunger >= maxHunger && health != null && health.currentHealth < health.maxHealthValue) 
        {
            health.Heal(healValue);
        }

        hunger = Mathf.Min(maxHunger, hunger + foodValue);
    }
}
