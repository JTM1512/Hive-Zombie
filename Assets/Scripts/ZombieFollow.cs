using UnityEngine;

public class ZombieFollow : MonoBehaviour
{
    [SerializeField] float followSpeed = 6f; // Speed at which the zombie follows the player
    [SerializeField] float stopDistance = 1.5f; // Distance at which the zombie stops from the player

    Transform leader;
    bool isFollowing;

    public bool IsFollowing => isFollowing;

    // Method to start following the player
    public void StartFollowing(Transform playerTransform)
    {
        leader = playerTransform;
        isFollowing = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFollowing || leader == null) return;

        float distance = Vector2.Distance(transform.position, leader.position);
        if (distance <= stopDistance) return;

        // Move the zombie towards the player
        transform.position = Vector2.MoveTowards(transform.position, leader.position, followSpeed * Time.deltaTime);

    }
}
