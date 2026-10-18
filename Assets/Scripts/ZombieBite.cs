using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieBite : MonoBehaviour
{
    [SerializeField] private float biteRange = 1.5f; // Range within which the zombie can bite
    ZombieHunger hunger; // Reference to the ZombieHunger script

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hunger = GetComponent<ZombieHunger>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) // Check if the E key was pressed
        {
            Bite(); // Call the Bite method
        }
    }

    void Bite()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, biteRange);
        Collider2D closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (!CanBite(hit)) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            closest = hit;
        }

        if (closest == null) return;
        if (TryBiteCivilian(closest)) return;
        TryBiteEnemy(closest);
    }

    bool CanBite(Collider2D hit)
    {
        Civilian civilian = hit.GetComponent<Civilian>();
        if (civilian != null && !civilian.HasTurned)
            return true;

        EnemyUnit enemy = hit.GetComponent<EnemyUnit>();
        return enemy != null && !enemy.HasTurned;
    }

    bool TryBiteCivilian(Collider2D hit)
    {
        Civilian civilian = hit.GetComponent<Civilian>();
        if (civilian == null || civilian.HasTurned)
            return false;

        civilian.getBitten(hunger);
        if (civilian.HasTurned)
            PlayBite();
        return true;
    }

    bool TryBiteEnemy(Collider2D hit)
    {
        EnemyUnit enemy = hit.GetComponent<EnemyUnit>();
        if (enemy == null || enemy.HasTurned)
            return false;

        enemy.getBitten(hunger);
        if (enemy.HasTurned)
            PlayBite();
        return true;
    }

    void PlayBite()
    {
        ZombiePlayerAnimation animationDriver = GetComponent<ZombiePlayerAnimation>();
        if (animationDriver != null)
            animationDriver.PlayAttack();
    }

}
