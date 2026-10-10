using UnityEngine;

public static class Noise
{
    public static float Evalute(Vector3 point)
    {
        float x = point.x;
        float y = point.y;
        float z = point.z;

        float skew = (x + y + z) / 3f;

        int i = Mathf.FloorToInt(x + skew);
        int j = Mathf.FloorToInt(y + skew);
        int k = Mathf.FloorToInt(z + skew);

        return 0f;
    }
}
