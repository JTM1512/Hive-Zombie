using UnityEngine;

public class Civilian : MonoBehaviour
{
    [SerializeField] float food = 25f;
    [SerializeField] float heal = 20f;
    [SerializeField] Sprite zombieSprite;
    [SerializeField] float zombieHealth = 50f;

    bool turned;
    public bool HasTurned => turned;

    public void getBitten(ZombieHunger zombieHunger)
    {
        if (turned || zombieHunger == null) return;

        turned = true;

        zombieHunger.Eat(food, heal);

        // Change the civilian's sprite to the zombie sprite
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && zombieSprite != null)
        {
            spriteRenderer.sprite = zombieSprite;
        }

        // Health starts now. A living civilian does not have any.
        ZombieHealth health = gameObject.AddComponent<ZombieHealth>();
        health.SetHealth(zombieHealth);

        // Start following the player
        ZombieFollow zombieFollow = GetComponent<ZombieFollow>();
        if (zombieFollow != null)
        {
            zombieFollow.StartFollowing(zombieHunger.transform);
        }

        Collider2D body = GetComponent<Collider2D>();
        if (body != null)
        {
            body.isTrigger = true;
        }

        if (GetComponent<ZombieHealth>() == null)
        {
            gameObject.AddComponent<ZombieHealth>();
        }

        Tracker.Instance.AddZombie();
    }
    // Awake is called when the script instance is being loaded
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
