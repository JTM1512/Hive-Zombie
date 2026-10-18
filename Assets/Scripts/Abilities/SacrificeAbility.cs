using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SacrificeAbility : MonoBehaviour, IAbilityInfo
{
    public string DisplayName => "Sacrifice";
    public string KeyLabel => "2";
    public int ZombieCost => sacrificeCost;
    [SerializeField] int sacrificeCost = 3; // Number of zombies to sacrifice
    [SerializeField] float foodValue = 25f; // Amount of food to gain from the sacrifice
    [SerializeField] float healValue = 20f; // Amount of health to gain from the sacrifice

    Horde horde;
    ZombieHunger hunger; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horde = GetComponent<Horde>();
        hunger = GetComponent<ZombieHunger>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (Keyboard.current == null || !Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            return; 
        }

        List<ZombieFollow> spent = new List<ZombieFollow>();
        if (horde == null || !horde.TrySpend(sacrificeCost, spent)) return;

        // Apply the effects of the sacrifice to the player
        foreach (ZombieFollow zombie in spent)
        {
            if (hunger != null)
            {
                // Apply the food and heal values to the player's hunger system
                hunger.Eat(foodValue, healValue);
            }

            zombie.DespawnWithDeathAnimation();
        }
    }
}
