using UnityEngine;

public class Food : MonoBehaviour
{
    public Transform foodPrefab;
    private float minX, maxX, minY, maxY;

    //Sets game bounds
    private void Start()
    {
        float offset = 0.5f;

        minX = -50 + offset;
        maxX = 50 - offset;
        minY = -20 + offset;
        maxY = 20 - offset;

        if (foodPrefab != null)
            SpawnFood(3);
    }

    private void SpawnFood(int n){
        Debug.Log("Spawn");
        for (int i = 0; i < n; i++)
        {
            Transform food = Instantiate(foodPrefab);
            food.position = RandomPosition();
            Debug.Log($"Spawned food at {food.position}");
        }
    }
    
    //Return random food coordinates
    private Vector3 RandomPosition()
    {
        Vector3 newPosition;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        newPosition = new Vector3(x, y);     

        return newPosition;
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.tag == "Agent")
    //         this.transform.position = RandomPosition();
    // }
}
