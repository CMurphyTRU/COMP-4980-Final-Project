using UnityEngine;

// creates a line of terrain heights for the border between two planet faces.
public class DiamondSquareEdgeGenerator
{
    // number of height samples along the edge
    private int resolution;

    // how quickly the random changes get smaller as the line is subdivided
    private float roughness;

    // constructor
    public DiamondSquareEdgeGenerator(int resolution, float roughness)
    {
        this.resolution = resolution;
        this.roughness = roughness;
    }

    // generate a height for every point between two fixed endpoint heights
    public float[] GenerateEdge(float startHeight, float endHeight)
    {
        // create an array to hold the entire edge
        float[] heights = new float[resolution];

        // keep the given heights at the start and end points
        heights[0] = startHeight;
        heights[resolution - 1] = endHeight;

        // begin with one segment stretching across the whole edge
        int stepSize = resolution - 1;

        // the initial maximum size of a random height change
        float offset = 1f;

        // keep splitting segments until there are no empty midpoint positions left
        while (stepSize > 1)
        {
            // find the middle of the current segment
            int half = stepSize / 2;

            // visit each segment of the current size
            for (int i = 0; i < resolution - 1; i += stepSize)
            {
                // average the two endpoints, then add a random change to create the midpoint
                float average = (heights[i] + heights[i + stepSize]) / 2f;
                heights[i + half] = average + Random.Range(-offset, offset);
            }

            // use segments half as long as the next pass
            stepSize /= 2;

            // reduce the random variation each pass, creating finer terrain detail
            offset *= roughness;
        }

        // return the completed
        return heights;
    }
}
