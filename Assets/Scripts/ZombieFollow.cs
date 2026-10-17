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
    private bool isFollowing;
    private bool isDying;
    private float nextAttackTime;
    private ZombieHealth health;
    private Rigidbody2D body;
    private ZombiePlayerAnimation animationDriver;

    public void StartFollowing(Transform playerTransform)
    {
        leader = playerTransform;
        isFollowing = true;
        health = GetComponent<ZombieHealth>();
        animationDriver = GetComponent<ZombiePlayerAnimation>();
        nextAttackTime = Time.time;
    }

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animationDriver = GetComponent<ZombiePlayerAnimation>();
    }

    private void Update()
    {
        if (!isFollowing || leader == null || isDying) return;

        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame && health != null)
            health.TakeDamage(testDamage);

        if (health != null && health.currentHealth <= 0f)
            StartCoroutine(PlayDeathThenDisable());
    }

    private void FixedUpdate()
    {
        if (!isFollowing || leader == null || isDying) return;

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
}
