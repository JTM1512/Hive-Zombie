using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Horde : MonoBehaviour
{
    readonly List<ZombieFollow> freeZombies = new List<ZombieFollow>();

    // A new follower is added to the list of free zombies. This allows the horde to keep track of all available zombies that can be assigned to follow the player.
    public void Register(ZombieFollow zombie)
    {
        if (!freeZombies.Contains(zombie))
        {
            freeZombies.Add(zombie);
        }
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.qKey.wasPressedThisFrame) return;
        if (!TrySpend(1, spent)) return; 
        foreach (ZombieFollow zombie in spent)
        {
            zombie.gameObject.SetActive(false);
        }
    }
}
