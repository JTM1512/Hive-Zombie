using UnityEngine;

public class GrenadeBlast : MonoBehaviour
{
    const string sortingLayer = "Default";
    const float flashDuration = 0.25f;

    static Sprite circle;

    EnemyUnit shooter;
    Transform player;
    float radius;
    float damage;
    float fuse;
    float flashTime;
    bool exploded;
    SpriteRenderer marker;

    public static void Throw(EnemyUnit shooter, Transform player, Vector2 point, float radius, float damage, float fuse)
    {
        if (radius <= 0f) return;

        GameObject blastObject = new GameObject("GrenadeBlast");
        blastObject.transform.position = point;
        blastObject.transform.localScale = Vector3.one * (radius * 2f);

        SpriteRenderer marker = blastObject.AddComponent<SpriteRenderer>();
        marker.sprite = Circle();
        marker.color = new Color(1f, 0.35f, 0.05f, 0.35f);
        marker.sortingLayerName = sortingLayer;
        marker.sortingOrder = short.MaxValue - 1;

        GrenadeBlast blast = blastObject.AddComponent<GrenadeBlast>();
        blast.shooter = shooter;
        blast.player = player;
        blast.radius = radius;
        blast.damage = damage;
        blast.fuse = fuse;
        blast.marker = marker;
    }

    void Update()
    {
        if (!exploded)
        {
            fuse -= Time.deltaTime;
            if (fuse > 0f) return;
            Explode();
            return;
        }

        flashTime -= Time.deltaTime;
        float fade = Mathf.Clamp01(flashTime / flashDuration);
        Color color = marker.color;
        color.a = 0.85f * fade;
        marker.color = color;
        if (flashTime <= 0f)
            Destroy(gameObject);
    }

    void Explode()
    {
        exploded = true;
        flashTime = flashDuration;
        marker.color = new Color(1f, 0.7f, 0.15f, 0.85f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            if (shooter != null && hit.transform == shooter.transform) continue;
            TryHit(hit);
        }
    }

    bool TryHit(Collider2D collider)
    {
        if (collider == null) return false;

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

    static Sprite Circle()
    {
        if (circle != null) return circle;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        float radius = (size - 1) * 0.5f;
        Vector2 center = new Vector2(radius, radius);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();

        circle = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        circle.hideFlags = HideFlags.HideAndDontSave;
        return circle;
    }
}
