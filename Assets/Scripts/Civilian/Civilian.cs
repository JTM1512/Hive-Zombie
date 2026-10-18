using UnityEngine;

public class Civilian : MonoBehaviour
{
    [SerializeField] private float food = 25f;
    [SerializeField] private float heal = 20f;
    [SerializeField] private Sprite zombieSprite;
    [SerializeField] private float zombieHealth = 50f;

    private bool turned;
    public bool HasTurned => turned;

    public void getBitten(ZombieHunger zombieHunger)
    {
        if (turned || zombieHunger == null) return;
        turned = true;

        zombieHunger.Eat(food, heal);

        ZombiePlayerAnimation animationDriver = GetComponent<ZombiePlayerAnimation>();
        if (animationDriver != null)
        {
            CivilianAnimation civilianAnimation = GetComponent<CivilianAnimation>();
            if (civilianAnimation != null)
                civilianAnimation.enabled = false;
            animationDriver.StartZombieAnimations();
        }
        else
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && zombieSprite != null)
                spriteRenderer.sprite = zombieSprite;
        }

        ZombieHealth health = GetComponent<ZombieHealth>();
        if (health == null)
            health = gameObject.AddComponent<ZombieHealth>();
        health.SetHealth(zombieHealth);

        ZombieFollow zombieFollow = GetComponent<ZombieFollow>();
        if (zombieFollow != null)
            zombieFollow.StartFollowing(zombieHunger.transform);

        Collider2D body = GetComponent<Collider2D>();
        if (body != null)
            body.isTrigger = true;

        if (Tracker.Instance != null)
            Tracker.Instance.AddZombie();
    }
}
