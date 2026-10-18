using System.Collections;
using UnityEngine;

// A bitten civilian. Follows you, circles for the shield, or chases for the attack.
public class ZombieFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField, Min(0.1f)] private float obstacleLookAhead = 0.9f;

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
    // True once this zombie has reached the person and the hold timer has started.
    private bool attackHolding;
    // This zombie's own spot in the horde, relative to the player.
    private Vector2 slotOffset;
    private ZombieHealth health;
    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private ZombiePlayerAnimation animationDriver;
    private bool followingObstacleEdge;
    private int obstacleSide;
    private Vector2 obstacleTravelDirection;

    public bool IsFollowing => isFollowing;

    // Called by the civilian when the bite lands.
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

            // Take a free random spot so followers do not stack.
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

        // Shield time is up, so this zombie leaves.
        if (shielding)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f) BeginDeath();
            return;
        }

        if (attacking)
        {
            // The target is gone.
            if (attackTarget == null)
            {
                BeginDeath();
                return;
            }

            // Still chasing. The 5 seconds start after the grab.
            if (!attackHolding) return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f) BeginDeath();
        }
    }

    private void FixedUpdate()
    {
        if (isDying || body == null) return;

        // Walk around the player for the shield.
        if (shielding && leader != null)
        {
            angle += orbitSpeed * Time.fixedDeltaTime;
            Vector2 destination = GetLeaderPosition() + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
            Vector2 displacement = destination - body.position;
            MoveInDirection(displacement, displacement.magnitude);
            return;
        }

        if (attacking && attackTarget != null)
        {
            // Close enough to count as a grab.
            if (!attackHolding && Vector2.Distance(body.position, attackTarget.position) <= 1.2f)
            {
                attackHolding = true;
            }


            if (attackHolding)
            {
                // Stay on them and keep the pin refreshed.
                body.linearVelocity = Vector2.zero;
                body.MovePosition(attackTarget.position);
                Pinned pinned = attackTarget.GetComponent<Pinned>();
                if (pinned == null)
                {
                    pinned = attackTarget.gameObject.AddComponent<Pinned>();
                }
                   
                pinned.Hold();
                return;
            }

            // Not there yet. Keep running at the target.
            Vector2 displacement = (Vector2)attackTarget.position - body.position;
            float step = Mathf.Min(displacement.magnitude, attackSpeed * Time.fixedDeltaTime);
            MoveInDirection(displacement, step);
            return;
        }

        if (!isFollowing || leader == null) return;

        // Walk to this zombie's own spot beside the player.
        Vector2 current = body.position;
        Vector2 goal = GetLeaderPosition() + slotOffset;
        Vector2 delta = goal - current;
        if (delta.sqrMagnitude <= stopDistance * stopDistance)
        {
            animationDriver?.SetMovementDirection(Vector2.zero);
            return;
        }

        MoveInDirection(delta, followSpeed * Time.fixedDeltaTime);
    }

    // Moves toward a direction, but stops short of a wall.
    private Vector2 MoveInDirection(Vector2 desiredDisplacement, float maxDistance)
    {
        if (desiredDisplacement.sqrMagnitude <= 0.0001f || maxDistance <= 0f)
        {
            animationDriver?.SetMovementDirection(Vector2.zero);
            return body.position;
        }

        Vector2 direction = GetObstacleAwareDirection(body.position, desiredDisplacement.normalized);
        float safeDistance = GetClearance(body.position, direction, maxDistance);
        if (safeDistance <= 0.01f)
        {
            animationDriver?.SetMovementDirection(Vector2.zero);
            return body.position;
        }

        Vector2 destination = body.position + direction * Mathf.Min(maxDistance, safeDistance);
        MoveFollower(destination);
        return destination;
    }

    // If a wall is ahead, slide along it instead of walking into it.
    private Vector2 GetObstacleAwareDirection(Vector2 current, Vector2 desiredDirection)
    {
        float lookAhead = Mathf.Max(obstacleLookAhead, followSpeed * Time.fixedDeltaTime);
        bool blocked = TryGetObstacle(current, desiredDirection, lookAhead, out RaycastHit2D blockingHit);
        if (followingObstacleEdge)
        {
            if (!blocked)
            {
                followingObstacleEdge = false;
                obstacleSide = 0;
                return desiredDirection;
            }

            // Keep a stable direction along the obstacle instead of choosing a
            // new side every physics frame. At corners, bend along the new face.
            if (GetClearance(current, obstacleTravelDirection, lookAhead) > 0.05f)
                return obstacleTravelDirection;

            Vector2 cornerNormal = blockingHit.normal.sqrMagnitude > 0.01f
                ? blockingHit.normal.normalized
                : -desiredDirection;
            Vector2 cornerTangent = new Vector2(-cornerNormal.y, cornerNormal.x) * obstacleSide;
            if (GetClearance(current, cornerTangent, lookAhead) > 0.05f)
                obstacleTravelDirection = cornerTangent;
            return obstacleTravelDirection;
        }

        if (!blocked)
            return desiredDirection;

        Vector2 normal = blockingHit.normal.sqrMagnitude > 0.01f
            ? blockingHit.normal.normalized
            : -desiredDirection;
        Vector2 tangent = new Vector2(-normal.y, normal.x);
        Vector2 firstOption = (tangent + normal * 0.35f).normalized;
        Vector2 secondOption = (-tangent + normal * 0.35f).normalized;
        float firstClearance = GetClearance(current, firstOption, lookAhead);
        float secondClearance = GetClearance(current, secondOption, lookAhead);

        obstacleSide = firstClearance >= secondClearance ? 1 : -1;
        obstacleTravelDirection = obstacleSide == 1 ? firstOption : secondOption;
        followingObstacleEdge = true;
        return obstacleTravelDirection;
    }

    // How far this zombie can move before hitting something.
    private float GetClearance(Vector2 origin, Vector2 direction, float distance)
    {
        return TryGetObstacle(origin, direction, distance, out RaycastHit2D hit)
            ? Mathf.Max(0f, hit.distance - 0.03f)
            : distance;
    }

    // Looks ahead for the nearest real wall.
    private bool TryGetObstacle(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D closestHit)
    {
        closestHit = default;
        bool found = false;
        float closestDistance = distance;
        float radius = 0.3f;
        if (bodyCollider != null)
        {
            Vector3 extents = bodyCollider.bounds.extents;
            origin = bodyCollider.bounds.center;
            radius = new Vector2(extents.x, extents.y).magnitude;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, radius, direction, distance, obstacleLayers);
        foreach (RaycastHit2D hit in hits)
        {
            if (!IsObstacle(hit.collider)) continue;
            if (!found || hit.distance < closestDistance)
            {
                closestHit = hit;
                closestDistance = hit.distance;
                found = true;
            }
        }
        return found;
    }

    // Walls count. The player, other followers, and the chase target do not.
    private bool IsObstacle(Collider2D hitCollider)
    {
        if (hitCollider == null || hitCollider.isTrigger || hitCollider.attachedRigidbody == body)
            return false;
        if (leader != null && (hitCollider.transform == leader || hitCollider.transform.IsChildOf(leader)))
            return false;
        // The person being chased is the target, not a wall.
        if (attackTarget != null && (hitCollider.transform == attackTarget || hitCollider.transform.IsChildOf(attackTarget)))
            return false;
        return hitCollider.GetComponentInParent<ZombieFollow>() == null;
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

        // Only drop the horde count once.
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

    // Spent on an ability. They leave the free horde, but stay in the scene.
    public void MarkSpent()
    {
        ReleaseMySlot();
        isFollowing = false;
        // Horde.TrySpend already removed this follower from the tracker.
        deathCounted = true;
        animationDriver?.SetMovementDirection(Vector2.zero);
    }

    public void DespawnWithDeathAnimation()
    {
        BeginDeath();
    }

    // Give the horde spot back so another zombie can use it.
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
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        if (animationDriver == null) animationDriver = GetComponent<ZombiePlayerAnimation>();
    }

    // Shield ability. Circle the player until the timer ends.
    public void StartShield(float duration, float radius, float speed, float startAngle)
    {
        shielding = true;
        shieldTimer = duration;
        orbitRadius = radius;
        orbitSpeed = speed;
        angle = startAngle;
    }

    // Attack ability. Chase first, then hold the target.
    public void StartAttack(Transform target, float duration, float speed)
    {
        attackTarget = target;
        attacking = true;
        attackHolding = false;
        attackTimer = duration;
        attackSpeed = speed;
    }
}
