using System.Collections.Generic;
using UnityEngine;

public class CivilianSpawner : MonoBehaviour
{
    [SerializeField] GameObject civilianPrefab;
    [SerializeField] int count = 50;
    [SerializeField] Transform[] areas;
    [SerializeField] Vector2 areaSize = new Vector2(10, 10);
    [SerializeField] float minGap = 2f;
    [SerializeField] Transform player;
    [SerializeField] float playerClearance = 8f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Vector2> used = new List<Vector2>();
        int placed = 0;
        int attempts = 0;

        while (placed < count && attempts < count * 30)
        {
            attempts++;
            if (areas == null || areas.Length == 0) return;

            Transform area = areas[Random.Range(0, areas.Length)];
            Vector2 point = (Vector2)area.position + new Vector2(Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f), Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f));

            if (player != null && Vector2.Distance(point, player.position) < playerClearance) continue;

            bool crowded = false; 

            foreach (Vector2 other in used)
            {
                if (Vector2.Distance(point, other) < minGap)
                {
                    crowded = true;
                    break; 
                }
            }

            if (crowded) continue;

            // Spawn civilian in random chosen position
            Instantiate(civilianPrefab, point, Quaternion.identity);
            used.Add(point);
            placed++;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (areas == null) return;

        Gizmos.color = Color.yellow;
        foreach (Transform area in areas)
        {
            if (area != null)
            {
                Gizmos.DrawWireCube(area.position, areaSize);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
