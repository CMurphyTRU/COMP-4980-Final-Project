/*using UnityEngine;

public static class Noise
{
    static readonly Vector3[] gradients = {
        new Vector3(1, 1, 0),
        new Vector3(-1, 1, 0),
        new Vector3(1, -1, 0),
        new Vector3(-1, -1, 0),

        new Vector3(1, 0, 1),
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, -1),
        new Vector3(-1, 0, -1),

        new Vector3(0, 1, 1),
        new Vector3(0, -1, 1),
        new Vector3(0, 1, -1),
        new Vector3(0, -1, -1)

    };

    static float CornerContribution(Vector3 gradient, Vector3 displacement)
    {
        float t = 0.6f - displacement.sqrMagnitude;

        if (t <= 0f)
        {
            return 0f;
        }

        float dot = Vector3.Dot(gradient, displacement);

        return t * t * t * t * dot;
    }

    static int Hash(int x, int y, int z)
    {
        unchecked
        {
            int hash = x * 374761393;
            hash += y * 668265263;
            hash += z * 2147483647;

            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash = hash ^ (hash >> 16);

            return hash & 0x7fffffff;
        }
    }

    public static float Evalute(Vector3 point)
    {
        float x = point.x;
        float y = point.y;
        float z = point.z;

        float skew = (x + y + z) / 3f;

        int i = Mathf.FloorToInt(x + skew);
        int j = Mathf.FloorToInt(y + skew);
        int k = Mathf.FloorToInt(z + skew);

        float t = (i + j + k) / 6f;

        float x0 = x - (i - t);
        float y0 = y - (j - t);
        float z0 = z - (k - t);

        int i1, j1, k1;
        int i2, j2, k2;

        if (x0 >= y0)
        {
            if (y0 >= z0)
            {
                i1 = 1; j1 = 0; k1 = 0;
                i2 = 1; j2 = 1; k2 = 0;
            }
            else if (x0 >= z0)
            {
                i1 = 1; j1 = 0; k1 = 0;
                i2 = 1; j2 = 0; k2 = 1;
            }
            else
            {
                i1 = 0; j1 = 0; k1 = 1;
                i2 = 1; j2 = 0; k2 = 1;
            }
        }
        else
        {
            if (y0 < z0)
            {
                i1 = 0; j1 = 0; k1 = 1;
                i2 = 0; j2 = 1; k2 = 1;
            }
            else if (x0 < z0)
            {
                i1 = 0; j1 = 1; k1 = 0;
                i2 = 1; j2 = 1; k2 = 0;
            }
        }

        const float G3 = 1f / 6f;

        float x1 = x0 - i1 + G3;
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;

        float x2 = x0 - i2 + 2f * G3;
        float y2 = y0 - j2 + 2f * G3;
        float z2 = z0 - k2 + 2f * G3;

        float x3 = x0 - 0.5f;
        float y3 = y0 - 0.5f;
        float z3 = z0 - 0.5f;

        int g0 = Hash(i, j, k) % 12;
        int g1 = Hash(i + i1, j + j1, k + k1) % 12;
        int g2 = Hash(i + i2, j + j2, k + k2) % 12;
        int g3 = Hash(i + 1, j + 1, k + 1) % 12;

        float n0 = CornerContribution(
            gradient0,
            new Vector3(x0, y0, z0)
        );

        float n1 = CornerContribution(
            gradient1,
            new Vector3(x1, y1, z1)
        );

        float n2 = CornerContribution(
            gradient2,
            new Vector3(x2, y2, z2)
        );

        float n3 = CornerContribution(
            gradient3,
            new Vector3(x3, y3, z3)
        );


        return 32f * (n0, n1, n2, n3);
    }
}*/
