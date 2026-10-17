using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieFollow : MonoBehaviour
{
    [SerializeField] float followSpeed = 6f; // Speed at which the zombie follows the player
    [SerializeField] float stopDistance = 1.5f; // Distance at which the zombie stops from the player
    [SerializeField] float testDamage = 25f; // Damage to apply when the 'K' key is pressed for testing purposes

    Transform leader;
    bool isFollowing;
    ZombieHealth health;

    // Method to start following the player
    public void StartFollowing(Transform playerTransform)
    {
        leader = playerTransform;
        isFollowing = true;

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
            // Stop following the player and disable the zombie
            gameObject.SetActive(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, leader.position);
        if (distance <= stopDistance) return;

        // Move the zombie towards the player
        transform.position = Vector2.MoveTowards(transform.position, leader.position, followSpeed * Time.deltaTime);

    }
}
