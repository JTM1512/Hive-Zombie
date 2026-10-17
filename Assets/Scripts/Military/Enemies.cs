using UnityEngine;

[System.Serializable]
public class Enemies : MonoBehaviour
{
    [SerializeField] Tiers[] Tiers;
    [SerializeField] float spawnInterval = 8f;
    [SerializeField] int maxAlive = 6;
    [SerializeField] Transform[] spawnPoints;

    Tiers currentTier;
    float spawnTimer;
    int alive;
    bool listening;

    void Start()
    {
        Listen();
        if (Tracker.Instance == null) return;
        currentTier = PickTier(Tracker.Instance.Count);
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
        Tiers best = Tiers[0];
        foreach(Tiers tier in Tiers)
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
