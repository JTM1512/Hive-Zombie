using UnityEngine;
using System;

public class Tracker : MonoBehaviour
{
    public static Tracker Instance { get; private set; }
    public int Count{ get; private set; } = 1;
    public event Action<int> CountChanged;

    void Awake()
    {
        Instance = this;
    }
    public void AddZombie()
    {
        Count++;
        CountChanged?.Invoke(Count);
    }
    public void RemoveZombie()
    {
        Count = Mathf.Max(1, Count - 1);
        CountChanged?.Invoke(Count);
    }
}
