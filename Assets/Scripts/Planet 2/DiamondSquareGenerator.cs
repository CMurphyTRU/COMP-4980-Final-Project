using UnityEngine;

public class DiamondSquareGenerator : MonoBehaviour
{
    private int resolution;
    private float[,] heightMap;
    private bool[,] fixedPoints;
    private float roughness;

    public DiamondSquareGenerator(int resolution, float roughness)
    {
        this.resolution = resolution;
        this.roughness = roughness;

        heightMap = new float[resolution, resolution];
        fixedPoints = new bool[resolution, resolution];


    }

    public void GenerateHeights(float bottomLeft, float bottomRight, float topLeft, float topRight)
    {
        int last = resolution - 1;

        if (!fixedPoints[0, 0])
        {
            heightMap[0, 0] = bottomLeft;
        }

        if (!fixedPoints[last, 0])
        {
            heightMap[last, 0] = bottomRight;
        }

        if (!fixedPoints[0, last])
        {
            heightMap[0, last] = topLeft;
        }

        if (!fixedPoints[last, last])
        {
            heightMap[last, last] = topRight;
        }

        int stepSize = last;
        float offset = 1f;
        float roughness = 0.5f;

        while (stepSize > 1)
        {
            int half = stepSize / 2;

            for (int y = 0; y < last; y += stepSize)
            {
                for (int x = 0; x < last; x += stepSize)
                {
                    DiamondStep(x, y, stepSize, offset);
                }
            }

            for (int y = 0; y < resolution; y += half)
            {
                int startX = (y / half) % 2 == 0 ? half : 0;

                for (int x = startX; x < resolution; x += stepSize)
                {
                    SquareStep(x, y, half, offset);
                }
            }

            stepSize /= 2;
            offset *= roughness;
        }
    }

    private void DiamondStep(int x, int y, int stepSize, float offset)
    {
        int half = stepSize / 2;

        if (fixedPoints[x + half, y + half])
        {
            return;
        }

        float a = heightMap[x, y];
        float b = heightMap[x + stepSize, y];
        float c = heightMap[x, y + stepSize];
        float d = heightMap[x + stepSize, y + stepSize];

        float average = (a + b + c + d) / 4f;

        float displacement = Random.Range(-offset, offset);

        heightMap[x + half, y + half] = average + displacement;
    }

    private void SquareStep(int x, int y, int half, float offset)
    {
        if (fixedPoints[x,y])
        {
            return;
        }
        float sum = 0f;
        int count = 0;

        if (x - half >= 0)
        {
            sum += heightMap[x - half, y];
            count++;
        }

        if (x + half < resolution)
        {
            sum += heightMap[x + half, y];
            count++;
        }

        if (y - half >= 0)
        {
            sum += heightMap[x, y - half];
            count++;
        }

        if (y + half < resolution)
        {
            sum += heightMap[x, y + half];
            count++;
        }

        float average = sum / count;

        float displacement = Random.Range(-offset, offset);

        heightMap[x, y] = average + displacement;
    }

    public float GetHeight(int x, int y)
    {
        return heightMap[x, y];
    }

    public void SetFixedHeight(int x, int y, float height)
    {
        heightMap[x, y] = height;
        fixedPoints[x, y] = true;
    }

    public void NormalizeHeights(float minHeight, float maxHeight)
    {
        float range = maxHeight - minHeight;

        if (Mathf.Approximately(range, 0f))
        {
            return;
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                heightMap[x, y] = (heightMap[x, y] - minHeight) / range;
            }
        }
    }

    public void GetHeightRange(out float minHeight, out float maxHeight)
    {
        minHeight = float.MaxValue;
        maxHeight = float.MinValue;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float height = heightMap[x, y];

                if (height < minHeight)
                {
                    minHeight = height;
                }

                if (height > maxHeight)
                {
                    maxHeight = height;
                }
            }
        }
    }
}
