using UnityEngine;

public static class Utils
{
    public static Vector3 RandomPosition()
    {
        float offset = 0.5f;
        float minX = -50 + offset;
        float maxX = 50 - offset;
        float minY = -20 + offset;
        float maxY = 20 - offset;

        Vector3 newPosition;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        newPosition = new Vector3(x, y);     

        return newPosition;
    }
}
