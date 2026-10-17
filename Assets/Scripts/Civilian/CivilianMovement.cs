using UnityEngine;

public class CivilianMovement : MonoBehaviour
{
    [SerializeField] float noticeRange = 6f;
    [SerializeField] float fleeSpeed = 3.5f;
    [SerializeField] float wanderSpeed = 1.5f;

    Civilian civilian;
    float swerve;
    float nextTurn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        civilian = GetComponent<Civilian>();
        PickSwerve();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (civilian != null && civilian.HasTurned)
        {
            // If the civilian has turned into a zombie, stop moving. ZombieFollow will handle movement.
            return;
        }

        // Attack ability is keeping them in place, so don't move.
        Pinned pinned = GetComponent<Pinned>();
        if (pinned != null && pinned.IsPinned)
        {
            return;
        }

        // New swerve every fraction of a second, so they don't move in a straight line.
        if (Time.time >= nextTurn)
        {
            PickSwerve();
        }

        Transform threat = FindThreat();
        if (threat != null)
        {
           Vector2 away = ((Vector2)transform.position - (Vector2)threat.position).normalized;
           Vector2 direction = Quaternion.Euler(0, 0, swerve) * away;
           transform.position += (Vector3)(direction * fleeSpeed * Time.deltaTime);
            return; 

        }

        // No threat, so wander around.
        Vector2 wander = Quaternion.Euler(0, 0, swerve) * Vector2.right;
        transform.position += (Vector3)(wander * wanderSpeed * Time.deltaTime);

    }

    void PickSwerve()
    {
        swerve = Random.Range(-80f, 80f);
        nextTurn = Time.time + Random.Range(0.25f, 0.6f);
    }

    Transform FindThreat()
    {
        ZombieHealth[] zombies = FindObjectsByType<ZombieHealth>(FindObjectsSortMode.None);
        Transform closest = null;
        float closestDistance = noticeRange;

        foreach (ZombieHealth zombie in zombies)
        {
           if (zombie.gameObject == gameObject) continue; // Don't consider self as a threat.

           float distance = Vector2.Distance(transform.position, zombie.transform.position);
            if (distance >= closestDistance) continue; // Only consider zombies within notice range.

            closestDistance = distance;
            closest = zombie.transform;
        }

        return closest;
    }
}
