using UnityEngine;

public class Food : MonoBehaviour
{
    public Transform foodPrefab;

    public void SpawnFood(int n){
        for (int i = 0; i < n; i++)
        {
            Transform food = Instantiate(foodPrefab);
            food.position = Utils.RandomPosition();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Agent"){
            this.transform.position = Utils.RandomPosition();
        }
    }
}
