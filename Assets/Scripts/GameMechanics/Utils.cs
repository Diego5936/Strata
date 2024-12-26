using UnityEngine;

public class Utils
{    
    public static Vector3 RandomPosition()
    {
        float offset = 2;
        float minX = -100 + offset;
        float maxX = 100 - offset;
        float minY = -40 + offset;
        float maxY = 40 - offset;

        Vector3 newPosition;

        float x = UnityEngine.Random.Range(minX, maxX);
        float y = UnityEngine.Random.Range(minY, maxY);

        newPosition = new Vector3(x, y);     

        return newPosition;
    }
}
