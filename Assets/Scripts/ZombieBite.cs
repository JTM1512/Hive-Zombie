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
            if (civilian == null)
            {
                continue; // Skip if the collider is a civilian
            }
            civilian.getBitten(hunger); // Call the getBitten method on the civilian, passing the zombie's hunger
            break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // Set the gizmo color to red
        Gizmos.DrawWireSphere(transform.position, biteRange); // Draw a wireframe sphere to visualize the bite range
    }
}
