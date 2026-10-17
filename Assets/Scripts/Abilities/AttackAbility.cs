using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackAbility : MonoBehaviour
{
    [SerializeField] int cost = 2;
    [SerializeField] float searchRange = 8f;
    [SerializeField] float duration = 5f;
    [SerializeField] float chargeSpeed = 8f;

    Horde horde;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horde = GetComponent<Horde>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.digit3Key.wasPressedThisFrame) return;

        //No target in range so don't spend any zombies
        Transform target = FindTarget();
        if (target == null) return;

        List<ZombieFollow> spent = new List<ZombieFollow>();
        if (horde == null || !horde.TrySpend(cost, spent)) return;

        // Start the attack with each spent zombie
        foreach (ZombieFollow zombie in spent)
        {
            zombie.StartAttack(target, duration, chargeSpeed);
        }
    }

    Transform FindTarget()
    {
        Transform best = null;
        float bestDistance = searchRange;

        Civilian[] civilians = FindObjectsByType<Civilian>(FindObjectsSortMode.None);
        foreach (Civilian civilian in civilians)
        {
            // Already turned civilians are not valid targets
            if (civilian.GetComponent<ZombieHealth>() != null)
                continue;

            float distance = Vector2.Distance(transform.position, civilian.transform.position);
            if (distance >= bestDistance)
            {
               continue;
            }

            bestDistance = distance;
            best = civilian.transform;
        }

        EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
        foreach (EnemyUnit enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance >= bestDistance)
            {
                continue;
            }
            bestDistance = distance;
            best = enemy.transform;
        }

        return best;
    }
}
