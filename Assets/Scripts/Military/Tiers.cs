using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Tiers
{
    public string tierName;
    public int minZombies;
    public float health;
    public float damage;
    public float speed;
    public GameObject zombiePrefab;
    [SerializeField] public AttackType attackType;
    [SerializeField] public float range;
    [SerializeField] public float cooldown;
}
public enum AttackType {Baton, Pistol, Rifle, Burst, Grenade}
    
