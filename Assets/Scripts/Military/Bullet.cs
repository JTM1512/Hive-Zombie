using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    const string sortingLayer = "Default";

    static Sprite streak;

    EnemyUnit shooter;
    Transform player;
    float damage;
    Vector2 direction;
    float speed;
    float remaining;

    public static void Spawn(EnemyUnit shooter, Transform player, Vector2 origin, Vector2 direction, float range, float speed, float damage, Sprite projectileSprite = null)
    {
        if (direction.sqrMagnitude < 0.0001f || range <= 0f) return;

        GameObject bulletObject = new GameObject("Bullet");
        bulletObject.transform.position = origin;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        bulletObject.transform.localScale = projectileSprite != null
            ? Vector3.one * 2f
            : new Vector3(0.45f, 0.12f, 1f);

        SpriteRenderer renderer = bulletObject.AddComponent<SpriteRenderer>();
        renderer.sprite = projectileSprite != null ? projectileSprite : Streak();
        renderer.color = projectileSprite != null ? Color.white : new Color(1f, 0.85f, 0.2f);
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = short.MaxValue;

        Bullet bullet = bulletObject.AddComponent<Bullet>();
        bullet.shooter = shooter;
        bullet.player = player;
        bullet.damage = damage;
        bullet.direction = direction.normalized;
        bullet.speed = speed;
        bullet.remaining = range;
    }

    void Update()
    {
        float step = Mathf.Min(speed * Time.deltaTime, remaining);
        if (step <= 0f) return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, step);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (shooter != null && hit.transform == shooter.transform) continue;

            if (!hit.collider.isTrigger)
            {
                transform.position = hit.point;
                TryHit(hit.collider);
                Destroy(gameObject);
                return;
            }

            if (TryHit(hit.collider))
            {
                transform.position = hit.point;
                Destroy(gameObject);
                return;
            }
        }

        transform.position += (Vector3)(direction * step);
        remaining -= step;
        if (remaining <= 0f)
            Destroy(gameObject);
    }

    bool TryHit(Collider2D collider)
    {
        ZombieHealth zombie = collider.GetComponent<ZombieHealth>();
        if (CanHurt(zombie))
        {
            zombie.TakeDamage(damage);
            return true;
        }

        EnemyUnit ally = collider.GetComponent<EnemyUnit>();
        if (ally == null || ally == shooter) return false;

        ally.TakeDamage(damage);
        return true;
    }

    bool CanHurt(ZombieHealth zombie)
    {
        if (zombie == null) return false;
        if (player != null && zombie.transform == player) return true;

        ZombieFollow follow = zombie.GetComponent<ZombieFollow>();
        return follow != null && follow.IsFollowing;
    }

    static Sprite Streak()
    {
        if (streak != null) return streak;

        streak = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        streak.hideFlags = HideFlags.HideAndDontSave;
        return streak;
    }
}
