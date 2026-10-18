using UnityEngine;
using System;

public class Tracker : MonoBehaviour
{
    public static Tracker Instance { get; private set; }
    public int Count{ get; private set; } = 1;
    public event Action<int> CountChanged;

    // Highest horde ever reached. Stays saved after you restart.
    public int Best { get; private set; }
    const string BestKey = "BestHorde";

    void Awake()
    {
        Instance = this;
        Best = PlayerPrefs.GetInt(BestKey, 1);
    }
    public void AddZombie()
    {
        Count++;

        // Only save when this horde is bigger than the old record.
        if (Count > Best)
        {
            Best = Count;
            PlayerPrefs.SetInt(BestKey, Best);
            PlayerPrefs.Save();
        }

        CountChanged?.Invoke(Count);
    }
    public void RemoveZombie()
    {
        Count = Mathf.Max(1, Count - 1);
        CountChanged?.Invoke(Count);
    }
}
