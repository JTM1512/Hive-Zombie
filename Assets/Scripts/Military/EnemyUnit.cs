using System.Collections;
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
    [SerializeField] float food = 25f;
    [SerializeField] float heal = 20f;
    [SerializeField] float zombieHealth = 50f;
    [SerializeField] Sprite[] meleeIdleFrames;
    [SerializeField] Sprite[] meleeWalkFrames;
    [SerializeField, Min(0.01f)] float meleeIdleFrameDuration = 0.25f;
    [SerializeField, Min(0.01f)] float meleeWalkFrameDuration = 0.12f;
    [SerializeField] Sprite[] rangeRunFrames;
    [SerializeField] Sprite[] rangeFireFrames;
    [SerializeField] Sprite[] robotRunFrames;
    [SerializeField] Sprite[] robotFireFrames;
    [SerializeField] Sprite bulletSprite;
    [SerializeField, Min(0.01f)] float rangedFrameDuration = 0.12f;
    [SerializeField, Min(0.1f)] float bulletTravelSpeed = 14f;
    [SerializeField] float blastRadius = 2f;
    [SerializeField] float grenadeFuse = 1f;

    float attackTimer;
    float animationTimer;
    int animationFrame;
    bool wasMoving;
    bool turned;
    Enemies owner;
    Transform target;
    SpriteRenderer spriteRenderer;
    EnemyLook look = EnemyLook.Placeholder;

    enum EnemyLook { Placeholder, Baton, Range, Robot }

    public bool HasTurned => turned;

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
        damage = tier.damage * (tier.attackType == AttackType.Baton ? 5f : 1f);
        speed = tier.speed;
        target = Tracker.Instance.transform;
        attackType = tier.attackType;
        range = tier.range;
        cooldown = tier.cooldown;
        attackTimer = 0f;

        string tierName = tier.tierName ?? string.Empty;
        if (tierName.IndexOf("Stronger cops", System.StringComparison.OrdinalIgnoreCase) >= 0)
            look = EnemyLook.Range;
        else if (tierName.IndexOf("military", System.StringComparison.OrdinalIgnoreCase) >= 0)
            look = EnemyLook.Robot;
        else if (attackType == AttackType.Baton)
            look = EnemyLook.Baton;

        if ((look == EnemyLook.Range || look == EnemyLook.Robot) && spriteRenderer != null)
            spriteRenderer.color = Color.white;
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

    public void getBitten(ZombieHunger zombieHunger)
    {
        if (turned || zombieHunger == null) return;
        turned = true;

        zombieHunger.Eat(food, heal);
        if (owner != null)
            owner.NotifyDeath();

        if (GetComponent<Rigidbody2D>() == null)
            gameObject.AddComponent<Rigidbody2D>();

        ZombieHealth healthComponent = GetComponent<ZombieHealth>();
        if (healthComponent == null)
            healthComponent = gameObject.AddComponent<ZombieHealth>();
        healthComponent.SetHealth(zombieHealth);

        ZombieFollow follow = GetComponent<ZombieFollow>();
        if (follow == null)
            follow = gameObject.AddComponent<ZombieFollow>();
        follow.StartFollowing(zombieHunger.transform);

        Collider2D body = GetComponent<Collider2D>();
        if (body != null)
            body.isTrigger = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = Color.white;

        ZombiePlayerAnimation animationDriver = GetComponent<ZombiePlayerAnimation>();
        if (animationDriver != null)
            animationDriver.StartZombieAnimations();

        if (Tracker.Instance != null)
            Tracker.Instance.AddZombie();

        enabled = false;
    }

    void Update()
    {
        if (turned || target == null)
            return;
        Pinned pinned = GetComponent<Pinned>();
        if (pinned != null && pinned.IsPinned) return;

        float distance = Vector2.Distance(transform.position, target.position);
        bool moving = distance > range;
        if (look != EnemyLook.Placeholder)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = target.position.x < transform.position.x;
            UpdateLookAnimation(moving);
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

    void UpdateLookAnimation(bool moving)
    {
        if (spriteRenderer == null) return;

        Sprite[] frames;
        float frameDuration;
        switch (look)
        {
            case EnemyLook.Baton:
                frames = moving ? meleeWalkFrames : meleeIdleFrames;
                frameDuration = moving ? meleeWalkFrameDuration : meleeIdleFrameDuration;
                break;
            case EnemyLook.Range:
                frames = moving ? rangeRunFrames : rangeFireFrames;
                frameDuration = rangedFrameDuration;
                break;
            case EnemyLook.Robot:
                frames = moving ? robotRunFrames : robotFireFrames;
                frameDuration = rangedFrameDuration;
                break;
            default:
                return;
        }
        if (frames == null || frames.Length == 0) return;

        if (moving != wasMoving)
        {
            wasMoving = moving;
            animationFrame = 0;
            animationTimer = 0f;
        }

        animationTimer += Time.deltaTime;
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
                Fire(DirectionToTarget());
                break;
            case AttackType.Burst:
                FireBurst();
                break;
            case AttackType.Grenade:
                GrenadeBlast.Throw(this, target, target.position, blastRadius, damage, grenadeFuse);
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

    Vector2 DirectionToTarget()
    {
        return ((Vector2)target.position - (Vector2)transform.position).normalized;
    }

    void Fire(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        direction.Normalize();
        const float muzzle = 0.6f;
        Vector2 origin = (Vector2)transform.position + direction * muzzle;
        float travel = Mathf.Max(0.1f, range - muzzle);
        Bullet.Spawn(this, target, origin, direction, travel, bulletTravelSpeed, damage, bulletSprite);
    }

    void FireBurst()
    {
        Vector2 direction = DirectionToTarget();
        float[] angles = { -15f, 0f, 15f };

        foreach (float angle in angles)
        {
            Vector2 shot = Quaternion.Euler(0f, 0f, angle) * (Vector3)direction;
            Fire(shot);
        }
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
        if (turned) return;

        health = Mathf.Max(0f, health - amount);
        if (health > 0f) return;

        if (owner != null)
        {
            owner.NotifyDeath();
        }
        Destroy(gameObject);
    }
}
