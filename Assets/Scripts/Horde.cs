using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Horde : MonoBehaviour
{
    int nextSlot;

    readonly List<ZombieFollow> freeZombies = new List<ZombieFollow>();

    readonly List<Vector2> slots = new List<Vector2>();


    // A new follower is added to the list of free zombies. This allows the horde to keep track of all available zombies that can be assigned to follow the player.
    public int Register(ZombieFollow zombie)
    {
        if (!freeZombies.Contains(zombie))
        {
            freeZombies.Add(zombie);
            nextSlot++;
        }

        return nextSlot - 1;
    }

    // A follower is removed from the list of free zombies. This allows the horde to keep track of all available zombies that can be assigned to follow the player.
    public void Unregister(ZombieFollow zombie)
    {
            freeZombies.Remove(zombie);
    }

    public bool TrySpend(int cost, List<ZombieFollow> spent)
    {
        spent.Clear();

        // Count starts at 1 because only followers can be spent, and the player is not a floower.
        int available = Tracker.Instance.Count - 1;
        if (cost > available || cost > freeZombies.Count)
        {
            return false;
        }
        for (int i = 0; i < cost; i++)
        {
            ZombieFollow zombie = freeZombies[0];
            freeZombies.RemoveAt(0);
            zombie.MarkSpent();
            Tracker.Instance.RemoveZombie();
            spent.Add(zombie);
        }

        return true;

    }

    public Vector2 AssignSlot() 
    { 
        float gap = 1.1f; // Adjust this value to control the spacing between zombies
        float minRadius = 1.4f; // Minimum radius for the first zombie

        for (int attempt = 0; attempt < 40; attempt++)
        {
            float reach = 4f + slots.Count * 0.2f; // Increase reach as more zombies are added
            float radius = Random.Range(minRadius, reach);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 spot = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            bool blocked = false;
            foreach (Vector2 other in slots)
            {
                if (Vector2.Distance(spot, other) < gap)
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked) continue;

            slots.Add(spot);
            return spot;
        }

        // If no valid spot was found after 40 attempts, place the zombie in a fallback position that is further away from the center.
        Vector2 fallback = Random.insideUnitCircle.normalized * (minRadius + slots.Count * 0.35f);
        slots.Add(fallback);
        return fallback;
    }

    public void ReleaseSlot(Vector2 slot)
    {
        slots.Remove(slot);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    /*void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.qKey.wasPressedThisFrame) return;
        if (!TrySpend(1, spent)) return; 
        foreach (ZombieFollow zombie in spent)
        {
            zombie.gameObject.SetActive(false);
        }
    }*/
}
