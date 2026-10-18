using System.Collections;
using UnityEngine;

public class ZombieFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float stopDistance = 1.5f;

    private Transform leader;
    private Rigidbody2D leaderBody;
    private Transform attackTarget;
    private bool isFollowing;
    private bool shielding;
    private float shieldTimer;
    private float orbitRadius;
    private float orbitSpeed;
    private float angle;
    private bool attacking;
    private float attackTimer;
    private float attackSpeed;
    private bool isDying;
    private bool deathCounted;
    private bool holdingSlot;
    private Vector2 slotOffset;
    private ZombieHealth health;
    private Rigidbody2D body;
    private ZombiePlayerAnimation animationDriver;

    public bool IsFollowing => isFollowing;

    public void StartFollowing(Transform playerTransform)
    {
        leader = playerTransform;
        leaderBody = playerTransform != null ? playerTransform.GetComponent<Rigidbody2D>() : null;
        isFollowing = true;
        health = GetComponent<ZombieHealth>();
        body = GetComponent<Rigidbody2D>();
        animationDriver = GetComponent<ZombiePlayerAnimation>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        Horde horde = playerTransform != null ? playerTransform.GetComponent<Horde>() : null;
        if (horde != null)
        {
            horde.Register(this);
            slotOffset = horde.AssignSlot();
            holdingSlot = true;
        }
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (isDying) return;

        if (health != null && health.currentHealth <= 0f)
        {
            BeginDeath();
            return;
        }

        if (shielding)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f) gameObject.SetActive(false);
            return;
        }

        if (attacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f || attackTarget == null) gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (isDying || body == null) return;

        if (shielding && leader != null)
        {
            angle += orbitSpeed * Time.fixedDeltaTime;
            Vector2 destination = GetLeaderPosition() + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
            MoveFollower(destination);
            return;
        }

        if (attacking && attackTarget != null)
        {
            Vector2 destination = Vector2.MoveTowards(body.position, attackTarget.position, attackSpeed * Time.fixedDeltaTime);
            MoveFollower(destination);
            if (Vector2.Distance(destination, attackTarget.position) <= 0.4f)
            {
                Pinned pinned = attackTarget.GetComponent<Pinned>();
                if (pinned == null) pinned = attackTarget.gameObject.AddComponent<Pinned>();
                pinned.Hold();
            }
            return;
        }

        if (!isFollowing || leader == null) return;

        Vector2 current = body.position;
        Vector2 goal = GetLeaderPosition() + slotOffset;
        Vector2 delta = goal - current;
        if (delta.sqrMagnitude <= stopDistance * stopDistance)
        {
            animationDriver?.SetMovementDirection(Vector2.zero);
            return;
        }

        Vector2 next = Vector2.MoveTowards(current, goal, followSpeed * Time.fixedDeltaTime);
        MoveFollower(next);
    }

    private void MoveFollower(Vector2 destination)
    {
        Vector2 direction = destination - body.position;
        animationDriver?.SetMovementDirection(direction);
        body.MovePosition(destination);
    }

    private Vector2 GetLeaderPosition()
    {
        return leaderBody != null ? leaderBody.position : (Vector2)leader.position;
    }

    private void BeginDeath()
    {
        if (isDying) return;
        isDying = true;
        isFollowing = false;
        shielding = false;
        attacking = false;
        attackTarget = null;

        ReleaseMySlot();
        if (leader != null)
        {
            Horde horde = leader.GetComponent<Horde>();
            if (horde != null) horde.Unregister(this);
        }
        leader = null;
        leaderBody = null;

        if (transform.parent != null) transform.SetParent(null, true);
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        if (!deathCounted && Tracker.Instance != null)
        {
            Tracker.Instance.RemoveZombie();
            deathCounted = true;
        }
        StartCoroutine(PlayDeathThenDisable());
    }

    private IEnumerator PlayDeathThenDisable()
    {
        float duration = animationDriver != null ? animationDriver.PlayDeath() : 0.6f;
        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));
        gameObject.SetActive(false);
    }

    public void MarkSpent()
    {
        ReleaseMySlot();
        isFollowing = false;
        animationDriver?.SetMovementDirection(Vector2.zero);
    }

    private void ReleaseMySlot()
    {
        if (!holdingSlot || leader == null) return;
        Horde horde = leader.GetComponent<Horde>();
        if (horde != null) horde.ReleaseSlot(slotOffset);
        holdingSlot = false;
    }

    private void CacheComponents()
    {
        if (health == null) health = GetComponent<ZombieHealth>();
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (animationDriver == null) animationDriver = GetComponent<ZombiePlayerAnimation>();
    }

    public void StartShield(float duration, float radius, float speed, float startAngle)
    {
        shielding = true;
        shieldTimer = duration;
        orbitRadius = radius;
        orbitSpeed = speed;
        angle = startAngle;
    }

    public void StartAttack(Transform target, float duration, float speed)
    {
        attackTarget = target;
        attacking = true;
        attackTimer = duration;
        attackSpeed = speed;
    }
}
