using UnityEngine;

public class Civilian : MonoBehaviour
{
    [SerializeField] float food = 25f;
    [SerializeField] float heal = 20f;
    [SerializeField] Sprite zombieSprite; 

    bool turned;

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

        // Start following the player
        ZombieFollow zombieFollow = GetComponent<ZombieFollow>();
        if (zombieFollow != null)
        {
            zombieFollow.StartFollowing(zombieHunger.transform);
        }

        Collider2D body = GetComponent<Collider2D>();
        if (body != null)
        {
            body.enabled = false;
        }
        Tracker.Instance.AddZombie();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
