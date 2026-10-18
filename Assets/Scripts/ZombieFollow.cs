using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float testDamage = 25f;
    [SerializeField] private float attackInterval = 1.25f;

    private Transform leader;
    private Transform attackTarget;
    private bool isFollowing;
    private bool spent;
    private bool shielding;
    private float shieldTimer;
    private float orbitRadius;
    private float orbitSpeed;
    private float angle;
    private bool attacking;
    private float attackTimer;
    private float attackSpeed;
    private bool isDying;
    private float nextAttackTime;
    private ZombieHealth health;
    private Rigidbody2D body;
    private ZombiePlayerAnimation animationDriver;

    public bool IsFollowing => isFollowing;

    public void StartFollowing(Transform playerTransform)
    {
        leader = playerTransform;
        isFollowing = true;
        health = GetComponent<ZombieHealth>();
        body = GetComponent<Rigidbody2D>();
        animationDriver = GetComponent<ZombiePlayerAnimation>();
        nextAttackTime = Time.time;

        Horde horde = playerTransform.GetComponent<Horde>();
        if (horde != null)
            horde.Register(this);
    }

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animationDriver = GetComponent<ZombiePlayerAnimation>();
    }

    private void Update()
    {
        if (leader == null || Time.timeScale == 0f || isDying) return;

        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame && health != null)
            health.TakeDamage(testDamage);

        if (health != null && health.currentHealth <= 0f)
        {
            if (!spent)
            {
                if (Tracker.Instance != null)
                    Tracker.Instance.RemoveZombie();
                spent = true;
            }

            Horde horde = leader.GetComponent<Horde>();
            if (horde != null)
                horde.Unregister(this);

            StartCoroutine(PlayDeathThenDisable());
            return;
        }

        if (shielding)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                gameObject.SetActive(false);
                return;
            }

            angle += orbitSpeed * Time.deltaTime;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
            Vector2 nextPosition = (Vector2)leader.position + offset;
            if (animationDriver != null)
                animationDriver.SetMovementDirection(nextPosition - (Vector2)transform.position);
            transform.position = nextPosition;
            return;
        }

        if (attacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f || attackTarget == null)
            {
                gameObject.SetActive(false);
                return;
            }

            Vector2 currentPosition = transform.position;
            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, attackTarget.position, attackSpeed * Time.deltaTime);
            if (animationDriver != null)
                animationDriver.SetMovementDirection(nextPosition - currentPosition);
            transform.position = nextPosition;
            if (Vector2.Distance(transform.position, attackTarget.position) <= 0.4f)
            {
                Pinned pinned = attackTarget.GetComponent<Pinned>();
                if (pinned == null)
                    pinned = attackTarget.gameObject.AddComponent<Pinned>();
                pinned.Hold();
            }
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!isFollowing || leader == null || isDying || shielding || attacking) return;

        Vector2 current = body != null ? body.position : (Vector2)transform.position;
        float distance = Vector2.Distance(current, leader.position);
        if (distance <= stopDistance)
        {
            if (animationDriver != null && Time.time >= nextAttackTime)
            {
                animationDriver.PlayAttack();
                nextAttackTime = Time.time + attackInterval;
            }
            return;
        }

        Vector2 next = Vector2.MoveTowards(current, leader.position, followSpeed * Time.fixedDeltaTime);
        if (animationDriver != null)
            animationDriver.SetMovementDirection(next - current);
        if (body != null)
            body.MovePosition(next);
        else
            transform.position = next;
    }

    private IEnumerator PlayDeathThenDisable()
    {
        isDying = true;
        isFollowing = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        float duration = animationDriver != null ? animationDriver.PlayDeath() : 0.6f;
        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));
        gameObject.SetActive(false);
    }

    public void MarkSpent()
    {
        spent = true;
        isFollowing = false;
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
        if (animationDriver == null) animationDriver = GetComponent<ZombiePlayerAnimation>();
        if (animationDriver != null) animationDriver.PlayAttack();
    }
}
