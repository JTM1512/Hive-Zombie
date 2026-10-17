using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieFollow : MonoBehaviour
{
    [SerializeField] float followSpeed = 6f; // Speed at which the zombie follows the player
    [SerializeField] float stopDistance = 1.5f; // Distance at which the zombie stops from the player
    [SerializeField] float testDamage = 25f; // Damage to apply when the 'K' key is pressed for testing purposes

    Transform leader;
    bool isFollowing;
    bool spent;
    bool shielding;
    float shieldTimer;
    float orbitRadius;
    float orbitSpeed;
    float angle;
    ZombieHealth health;

    public bool IsFollowing => isFollowing;

    // Method to start following the player
    public void StartFollowing(Transform playerTransform)
    {
        leader = playerTransform;
        isFollowing = true;

        // Register this zombie with the horde
        Horde horde = playerTransform.GetComponent<Horde>();
        if (horde != null)
        {
            horde.Register(this);
        }

        health = GetComponent<ZombieHealth>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFollowing || leader == null) return;
        if (Time.timeScale == 0f) return;

        // Stand-in hit until something in the scene can hurt them.
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame && health != null)
            health.TakeDamage(testDamage);

        // Check if the zombie is dead
        if (health != null && health.currentHealth <= 0)
        {
            // Spent zombies are removed from the count
            if (!spent && Tracker.Instance != null)
            {
                Tracker.Instance.RemoveZombie();
            }

            Horde horde = leader.GetComponent<Horde>();
            if (horde != null)
            {
                horde.Unregister(this);
            }

            // Stop following the player and disable the zombie
            gameObject.SetActive(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, leader.position);
        if (distance <= stopDistance) return;

        // Move the zombie towards the player
        transform.position = Vector2.MoveTowards(transform.position, leader.position, followSpeed * Time.deltaTime);

    }

    public void MarkSpent() 
    {
        spent = true;
        isFollowing = false;
    }
}
