using UnityEngine;

public class DiamondSquareEdgeGenerator
{
    private int resolution;
    private float roughness;

    public DiamondSquareEdgeGenerator(int resolution, float roughness)
    {
        this.resolution = resolution;
        this.roughness = roughness;
    }

    public float[] GenerateEdge(float startHeight, float endHeight)
    {
        float[] heights = new float[resolution];

        heights[0] = startHeight;
        heights[resolution - 1] = endHeight;

        int stepSize = resolution - 1;
        float offset = 1f;

        while (stepSize > 1)
        {
            int half = stepSize / 2;

            for (int i = 0; i < resolution - 1; i += stepSize)
            {
                float average = (heights[i] + heights[i + stepSize]) / 2f;
                heights[i + half] = average + Random.Range(-offset, offset);
            }

            stepSize /= 2;
            offset *= roughness;
        }

        return heights;
    }
}
