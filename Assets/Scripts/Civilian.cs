using UnityEngine;

public class Civilian : MonoBehaviour
{
    [SerializeField] float food = 25f;
    [SerializeField] float heal = 20f;

    public void getBitten(ZombieHunger zombieHunger)
    {
        if (zombieHunger == null) return;

        zombieHunger.Eat(food, heal);

        // Disable the civilian game object after being bitten
        gameObject.SetActive(false);
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
