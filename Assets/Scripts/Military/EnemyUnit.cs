using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    float health;
    float damage;
    float speed;
    Enemies owner;
    Transform target;
    public void Setup(Tiers tier, Enemies spawner)
    {
        owner = spawner;
        health = tier.health;
        damage = tier.damage;
        speed = tier.speed;
        target = Tracker.Instance.transform;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        if(target == null) return;
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0, health - amount);
        if(health <= 0)return;
        owner.NotifyDeath();
        Destroy(gameObject);
    }
}
