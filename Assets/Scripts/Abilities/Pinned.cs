using UnityEngine;

public class Pinned : MonoBehaviour
{
    float pinnedUntil;
    // True if the civilian is currently pinned by a zombie, false otherwise
    public bool IsPinned => Time.time < pinnedUntil;

    // Refreshed while a zombie is holding the civilian. Lets go once the zombie is gone.
    public void Hold()
    {
        pinnedUntil = Time.time + 0.2f;
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
