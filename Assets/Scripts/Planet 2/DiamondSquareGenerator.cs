using UnityEngine;

// generates a two dimensional height map using diamond-square. Each height later moves one planet vertex outward
public class DiamondSquareGenerator
{
    // number of points along each side of the height map
    private int resolution;
    
    // stores the terrain height at every x,y grid position
    private float[,] heightMap;

    // marks border positions that must keep their already assigned heights
    private bool[,] fixedPoints;

    private float roughness;

    public DiamondSquareGenerator(int resolution, float roughness)
    {
        this.resolution = resolution;
        this.roughness = roughness;

        heightMap = new float[resolution, resolution];
        fixedPoints = new bool[resolution, resolution];


    }

    // fill the height map, starting from its corners and repeatedly creating smaller details
    public void GenerateHeights(float bottomLeft, float bottomRight, float topLeft, float topRight)
    {   
        // find the final valid index on either axis
        int last = resolution - 1;

        // only set a corner if an earlier shared-edge operation has not already fixed its value
        if (!fixedPoints[0, 0])
        {
            heightMap[0, 0] = bottomLeft;
        }

        // set the bottom right corner unless its fixed
        if (!fixedPoints[last, 0])
        {
            heightMap[last, 0] = bottomRight;
        }

        // sets the top left corner unless it is fixed
        if (!fixedPoints[0, last])
        {
            heightMap[0, last] = topLeft;
        }

        // sets the top right corner unless it is fixed
        if (!fixedPoints[last, last])
        {
            heightMap[last, last] = topRight;
        }

        // start with squares covering the whole grid
        int stepSize = last;

        // start with random height changes of up to 1 in either direction
        float offset = 1f;
        
        // used to shrink the offset
        float roughness = 0.5f;

        // repeat the diamond and square steps using smaller squares each time
        while (stepSize > 1)
        {   
            // find half of the current square size
            int half = stepSize / 2;

            // visit every large square and calculate its center height
            for (int y = 0; y < last; y += stepSize)
            {
                for (int x = 0; x < last; x += stepSize)
                {
                    // use the four corners to create the center point
                    DiamondStep(x, y, stepSize, offset);
                }
            }

            // visits the points halfway along the sides of the current squares
            for (int y = 0; y < resolution; y += half)
            {
                // alternate the starting X position between rows to select the side midpoints
                int startX = (y / half) % 2 == 0 ? half : 0;

                for (int x = startX; x < resolution; x += stepSize)
                {
                    // use neighboring heights to fill this side midpoint
                    SquareStep(x, y, half, offset);
                }
            }

            // shrink the square size for the next round of detail
            stepSize /= 2;

            // reduce the random changes as the algorithm works on smaller features
            offset *= roughness;
        }
    }
    
    // find the center of one square by averaging its four corners and adding randomness
    private void DiamondStep(int x, int y, int stepSize, float offset)
    {
        int half = stepSize / 2;

        // do not change this center if its height has already been fixed
        if (fixedPoints[x + half, y + half])
        {
            return;
        }   

        // read the heights of the four corners of the square
        float a = heightMap[x, y];
        float b = heightMap[x + stepSize, y];
        float c = heightMap[x, y + stepSize];
        float d = heightMap[x + stepSize, y + stepSize];

        // average the four corners
        float average = (a + b + c + d) / 4f;

        // add a small random height change
        float displacement = Random.Range(-offset, offset);

        // save the new center height
        heightMap[x + half, y + half] = average + displacement;
    }

    // calculate a side midpoint from its available left, right, bottom and top neighbors
    private void SquareStep(int x, int y, int half, float offset)
    {   
        // leave shared-edge heights unchanged
        if (fixedPoints[x,y])
        {
            return;
        }

        // keep a running total and count of valid neighboring heights
        float sum = 0f;
        int count = 0;

        // if a left neighbor exists, include its height
        if (x - half >= 0)
        {
            sum += heightMap[x - half, y];
            count++;
        }

        // if a right neighbor exists, include its height
        if (x + half < resolution)
        {
            sum += heightMap[x + half, y];
            count++;
        }

        // if a bottom neighbor exists, include its height
        if (y - half >= 0)
        {
            sum += heightMap[x, y - half];
            count++;
        }

        // if a top neighbor exists, include its height
        if (y + half < resolution)
        {
            sum += heightMap[x, y + half];
            count++;
        }

        // average the neighbors that actually exist
        float average = sum / count;

        // choose a random amount between a negative and positive offset
        float displacement = Random.Range(-offset, offset);

        // store the calculated height
        heightMap[x, y] = average + displacement;
    }

    // return the height at the requested grid coordinates
    public float GetHeight(int x, int y)
    {
        return heightMap[x, y];
    }

    // store a height that later generation steps are not allowed to replace
    public void SetFixedHeight(int x, int y, float height)
    {
        heightMap[x, y] = height;

        // mark this position as fixed, usually because it belongs to a shared face border
        fixedPoints[x, y] = true;
    }

    // converts heights to a common 0-to-1 scale using the supplied minimum and maximum
    public void NormalizeHeights(float minHeight, float maxHeight)
    {
        // find the difference between the highest and lowest heights
        float range = maxHeight - minHeight;

        // avoid dividing by zero
        if (Mathf.Approximately(range, 0f))
        {
            return;
        }

        // visit each point in the heightmap
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // subtract the minimum and divide by the total range. minimum becomes 0 and maximum becomes 1
                heightMap[x, y] = (heightMap[x, y] - minHeight) / range;
            }
        }
    }

    // find the lowest and highest values currently stored in the height map
    public void GetHeightRange(out float minHeight, out float maxHeight)
    {
        // start with extreme limits so the first real value replaces them
        minHeight = float.MaxValue;
        maxHeight = float.MinValue;

        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // read the height at this point
                float height = heightMap[x, y];

                // update the minumum if this height is lower
                if (height < minHeight)
                {
                    minHeight = height;
                }

                // update the maximum if this height is higher
                if (height > maxHeight)
                {
                    maxHeight = height;
                }
            }
        }
    }
}
