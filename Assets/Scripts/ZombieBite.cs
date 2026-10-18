using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieBite : MonoBehaviour
{
    [SerializeField] private float biteRange = 1.5f; // Range within which the zombie can bite
    ZombieHunger hunger; // Reference to the ZombieHunger script

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hunger = GetComponent<ZombieHunger>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) // Check if the E key was pressed
        {
            Bite(); // Call the Bite method
        }
    }

    void Bite()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, biteRange); // Get all colliders within the bite range

        foreach (Collider2D hit in hits)
        {
            Civilian civilian = hit.GetComponent<Civilian>(); // Check if the collider has a Civilian component

            // Check if marked civilian is an already turned zombie
            if (civilian == null || civilian.HasTurned)
            {
                continue; // Skip if the collider is a civilian
            }
            
            civilian.getBitten(hunger); // Call the getBitten method on the civilian, passing the zombie's hunger

            // Trigger the player's bite animation only when the bite actually turns a civilian.
            if (civilian.HasTurned)
            {
                ZombiePlayerAnimation animationDriver = GetComponent<ZombiePlayerAnimation>();
                if (animationDriver != null)
                    animationDriver.PlayAttack();
            }

            break;
        }
    }

}
