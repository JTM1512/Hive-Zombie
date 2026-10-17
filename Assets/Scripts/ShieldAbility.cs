using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldAbility : MonoBehaviour
{
    [SerializeField] int cost = 4; // The number of zombies required to activate the shield ability
    [SerializeField] float shieldDuration = 5f; // Duration of the shield in seconds
    [SerializeField] float radius = 5f; // Radius of the shield effect
    [SerializeField] float orbitSpeed = 3f; // Speed at which the shield orbits around the player

    Horde horde;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horde= GetComponent<Horde>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.digit1Key.wasPressedThisFrame) return;

        List<ZombieFollow> spent = new List<ZombieFollow>();
        if (horde == null || !horde.TrySpend(cost, spent)) return;

        for (int i = 0; i < spent.Count; i++)
        {
            float angle = (Mathf.PI * 2f / spent.Count) * i;
            spent[i].StartShield(duartion, radius, orbitSpeed, angle);
        }
    }
}
