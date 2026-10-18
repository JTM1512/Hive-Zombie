using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField] private string characterSortingLayer = "Default";
    [SerializeField] float health;
    [SerializeField] float damage;
    [SerializeField] float speed;
    [SerializeField] AttackType attackType;
    [SerializeField] float range;
    [SerializeField] float cooldown;
    [SerializeField] Sprite[] meleeIdleFrames;
    [SerializeField] Sprite[] meleeWalkFrames;
    [SerializeField, Min(0.01f)] float meleeIdleFrameDuration = 0.25f;
    [SerializeField, Min(0.01f)] float meleeWalkFrameDuration = 0.12f;

    float attackTimer;
    float animationTimer;
    int animationFrame;
    bool wasMoving;
    SpriteRenderer spriteRenderer;
    float fuse;
    Vector2 blastPoint;
    bool grenadeLive;
    Enemies owner;
    Transform target;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyCharacterSorting();
    }

    public void Setup(Tiers tier, Enemies spawner)
    {
        ApplyCharacterSorting();
        owner = spawner;
        health = tier.health;
        damage = tier.damage;
        speed = tier.speed;
        target = Tracker.Instance.transform;
        attackType = tier.attackType;
        range = tier.range;
        cooldown = tier.cooldown;
        attackTimer = cooldown;
    }

    void ApplyCharacterSorting()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sprite in renderers)
        {
            sprite.sortingLayerName = characterSortingLayer;
            sprite.sortingOrder = short.MaxValue;
        }
    }

    void Update()
    {
        if (target == null)
            return;
        Pinned pinned = GetComponent<Pinned>();
        if (pinned != null && pinned.IsPinned) return;


        if (grenadeLive)
        {
            fuse -= Time.deltaTime;
            if (fuse <= 0f)
            {
                grenadeLive = false;
                DamageCircle(blastPoint, 2f);
            }
        }

        float distance = Vector2.Distance(transform.position, target.position);
        bool moving = distance > range;
        if (attackType == AttackType.Baton)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;
            UpdateMeleeAnimation(moving);
        }

        if (moving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, target.position, speed * Time.deltaTime);
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        attackTimer = cooldown;
        Attack();
    }

    void UpdateMeleeAnimation(bool moving)
    {
        if (spriteRenderer == null) return;

        Sprite[] frames = moving ? meleeWalkFrames : meleeIdleFrames;
        if (frames == null || frames.Length == 0) return;

        if (moving != wasMoving)
        {
            wasMoving = moving;
            animationFrame = 0;
            animationTimer = 0f;
        }

        animationTimer += Time.deltaTime;
        float frameDuration = moving ? meleeWalkFrameDuration : meleeIdleFrameDuration;
        while (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;
            animationFrame = (animationFrame + 1) % frames.Length;
        }
        spriteRenderer.sprite = frames[animationFrame];
    }

    void Attack()
    {
        switch (attackType)
        {
            case AttackType.Baton:
                HitClosest(range);
                break;
            case AttackType.Pistol:
            case AttackType.Rifle:
                HitRay(false);
                break;
            case AttackType.Burst:
                HitBurst();
                break;
            case AttackType.Grenade:
                blastPoint = target.position;
                fuse = 1f;
                grenadeLive = true;
                break;
        }
    }

    void HitClosest(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        ZombieHealth closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.transform == transform) continue;

            ZombieHealth zombie = hit.GetComponent<ZombieHealth>();
            if (!CanHurt(zombie)) continue;

            float hitDistance = Vector2.Distance(transform.position, hit.transform.position);
            if (hitDistance >= closestDistance) continue;

            closestDistance = hitDistance;
            closest = zombie;
        }

        if (closest != null)
        {
            closest.TakeDamage(damage);
        }
    }

    void HitRay(bool pierce)
    {
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, range);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform == transform) continue;
            if (!TryDamage(hit.collider)) continue;

            if (!pierce) break;
        }
    }

    void HitBurst()
    {
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float[] angles = { -15f, 0f, 15f };

        foreach (float angle in angles)
        {
            Vector2 shot = Quaternion.Euler(0f, 0f, angle) * (Vector3)direction;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, shot, range);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == transform) continue;
                if (TryDamage(hit.collider)) break;
            }
        }
    }

    void DamageCircle(Vector2 point, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.transform == transform) continue;
            TryDamage(hit);
        }
    }

    // A shot or blast damages another soldier it hits, and the attacker is skipped.
    bool TryDamage(Collider2D collider)
    {
        ZombieHealth zombie = collider.GetComponent<ZombieHealth>();
        if (CanHurt(zombie))
        {
            zombie.TakeDamage(damage);
            return true;
        }

        EnemyUnit ally = collider.GetComponent<EnemyUnit>();
        if (ally == null || ally == this) return false;

        ally.TakeDamage(damage);
        return true;
    }

    bool CanHurt(ZombieHealth zombie)
    {
        if (zombie == null) return false;
        if (zombie.transform == target) return true;

        ZombieFollow follow = zombie.GetComponent<ZombieFollow>();
        return follow != null && follow.IsFollowing;
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0f, health - amount);
        if (health > 0f) return;

        if (owner != null)
        {
            owner.NotifyDeath();
        }
        Destroy(gameObject);
    }
}
