using UnityEngine;
using System;

public class Tracker : MonoBehaviour
{
    public static Tracker Instance { get; private set; }
    public int Count{ get; private set; } = 1;
    public event Action<int> CountChanged;

    // Biggest horde in this run. A restart starts this at 1 again.
    public int Best { get; private set; } = 1;

    void Awake()
    {
        Instance = this;
        
    }
    public void AddZombie()
    {
        Count++;
        // This run's biggest horde so far.
        if (Count > Best)
            Best = Count;
        CountChanged?.Invoke(Count);
    }
    public void RemoveZombie()
    {
        Count = Mathf.Max(1, Count - 1);
        CountChanged?.Invoke(Count);
    }
}
