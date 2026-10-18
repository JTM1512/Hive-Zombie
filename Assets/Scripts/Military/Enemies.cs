using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemies : MonoBehaviour
{
    [SerializeField] Tiers[] Tiers;
    [SerializeField] float spawnInterval = 8f;
    [SerializeField, Min(0.1f)] float minimumSpawnInterval = 2f;
    [SerializeField, Min(0f)] float spawnIntervalDecreasePerMinute = 0.5f;
    [SerializeField] int maxAlive = 6;
    [SerializeField] Transform[] spawnPoints;

    readonly List<Tiers> unlocked = new List<Tiers>();
    float spawnTimer;
    float elapsedGameTime;
    int alive;
    bool listening;

    void Start()
    {
        Listen();
        if (Tracker.Instance == null) return;
        RefreshUnlocked(Tracker.Instance.Count);
    }

    void OnEnable()
    {
        Listen();
    }

    void OnDisable()
    {
        if (!listening || Tracker.Instance == null) return;
        Tracker.Instance.CountChanged -= OnHordeCountChanged;
        listening = false;
    }

    void Listen()
    {
        if (listening || Tracker.Instance == null) return;
        Tracker.Instance.CountChanged += OnHordeCountChanged;
        listening = true;
    }

    void OnHordeCountChanged(int newCount)
    {
        RefreshUnlocked(newCount);
    }

    void Update()
    {
        elapsedGameTime += Time.deltaTime;
        if (unlocked.Count == 0) return;
        if (alive >= maxAlive) return;
        spawnTimer += Time.deltaTime;
        float currentSpawnInterval = Mathf.Max(
            minimumSpawnInterval,
            spawnInterval - elapsedGameTime * spawnIntervalDecreasePerMinute / 60f);
        if (spawnTimer < currentSpawnInterval) return;
        spawnTimer = 0f;
        SpawnZombie();
    }

    void RefreshUnlocked(int zombieCount)
    {
        unlocked.Clear();
        if (Tiers == null) return;

        foreach (Tiers tier in Tiers)
        {
            if (tier == null || tier.zombiePrefab == null) continue;
            if (zombieCount >= tier.minZombies)
                unlocked.Add(tier);
        }
    }

    void SpawnZombie()
    {
        if (unlocked.Count == 0 || spawnPoints == null || spawnPoints.Length == 0) return;

        Tiers tier = unlocked[Random.Range(0, unlocked.Count)];
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject zombie = Instantiate(tier.zombiePrefab, point.position, Quaternion.identity);

        EnemyUnit enemySprite = zombie.GetComponent<EnemyUnit>();
        if (enemySprite != null)
            enemySprite.Setup(tier, this);
        alive++;
    }

    public void NotifyDeath()
    {
        alive = Mathf.Max(0, alive - 1);
    }
}
