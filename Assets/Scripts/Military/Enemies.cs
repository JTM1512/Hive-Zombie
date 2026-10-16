using UnityEngine;

public class Enemies : MonoBehaviour
{
    [SerializeField] Tiers[] enemyTiers;
    [SerializeField] float spawnInterval = 8f;
    [SerializeField] int maxAlive = 6;
    [SerializeField] Transform[] spawnPoints;

    Tiers currentTier;
    float spawnTimer;
    int alive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTier = PickTier(Tracker.Instance.Count);
    }
    void onEnable()
    {
        Tracker.Instance.CountChanged += OnHordeCountChanged;
    }
    void onDisable()
    {
        if(Tracker.Instance != null)
        {
            Tracker.Instance.CountChanged -= OnHordeCountChanged;
        }
    }
    void OnHordeCountChanged(int newCount)
    {
        currentTier = PickTier(newCount);
    }
    // Update is called once per frame
    void Update()
    {
        if (currentTier == null || currentTier.zombiePrefab == null) return;
        if(alive >= maxAlive) return;
        spawnTimer += Time.deltaTime;
        if(spawnTimer < spawnInterval) return;
        spawnTimer = 0f;
        SpawnZombie();
    }
    Tiers PickTier(int zombieCount)
    {
        Tiers best = enemyTiers[0];
        foreach(Tiers tier in enemyTiers)
        {
         if(zombieCount >= tier.minZombies && tier.minZombies > best.minZombies)
         {
            best = tier;
         }
        }
        return best;
    }
    void SpawnZombie()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject zombie = Instantiate(currentTier.zombiePrefab, point.position, Quaternion.identity);

        EnemyUnit enemySprite = zombie.GetComponent<EnemyUnit>();
        if(enemySprite != null)
        {
            enemySprite.Setup(currentTier,this);
        }
        alive++;
    }
    public void NotifyDeath()
    {
        alive = Mathf.Max(0, alive - 1);
    }
}
