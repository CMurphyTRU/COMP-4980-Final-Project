using UnityEngine;

// Implements deterministic three-dimensional simplex noise.
internal sealed class Noise
{
    // Transform between ordinary cartesian space and the simplex lattice.
    private const double SkewingFactor = 1.0 / 3.0;
    private const double UnskewingFactor = 1.0 / 6.0;

    // Twelve possible gradient directions used at simplex corners.
    private static readonly Vector3[] GradientDirections =
    {
        new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0),
        new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1),
        new Vector3(0, 1, 1), new Vector3(0, -1, 1), new Vector3(0, 1, -1), new Vector3(0, -1, -1)
    };

    // Fixed permutation values determine pseudorandom gradients.
    private static readonly int[] OriginalPermutationTable =
    {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

    private readonly int[] permutationTable = new int[512];

    // Duplicates the 256-entry permutation table to simplify wrapped lookups.
    public Noise()
    {
        for (int i = 0; i < 256; i++)
        {
            permutationTable[i] = OriginalPermutationTable[i];
            permutationTable[i + 256] = OriginalPermutationTable[i];
        }
    }

    // Samples 3D simplex noise at the given point and returns a smoothly varying value, approximately between -1 and 1.
    public float Evaluate(Vector3 point)
    {
        double x = point.x;
        double y = point.y;
        double z = point.z;

        // Skew the input so the containing simplex cell can be found by flooring.
        double skew = (x + y + z) * SkewingFactor;
        int i = FloorToInteger(x + skew);
        int j = FloorToInteger(y + skew);
        int k = FloorToInteger(z + skew);

        // Undo the skew to measure the point relative to its first simplex corner.
        double unskew = (i + j + k) * UnskewingFactor;
        double x0 = x - (i - unskew);
        double y0 = y - (j - unskew);
        double z0 = z - (k - unskew);

        // Coordinate ordering identifies the two intermediate simplex corners.
        int i1, j1, k1, i2, j2, k2;
        if (x0 >= y0)
        {
            if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
            else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
        }
        else
        {
            if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
            else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
            else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
        }

        // Relative distances from the sample point to the remaining three corners.
        double x1 = x0 - i1 + UnskewingFactor;
        double y1 = y0 - j1 + UnskewingFactor;
        double z1 = z0 - k1 + UnskewingFactor;
        double x2 = x0 - i2 + 2.0 * UnskewingFactor;
        double y2 = y0 - j2 + 2.0 * UnskewingFactor;
        double z2 = z0 - k2 + 2.0 * UnskewingFactor;
        double x3 = x0 - 0.5;
        double y3 = y0 - 0.5;
        double z3 = z0 - 0.5;

        // Wrap lattice coordinates into the permutation table's 256-value period.
        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;

        // Compute and sum the smoothly attenuated gradient contributions at each corner.
        double n0 = CornerContribution(ii, jj, kk, x0, y0, z0);
        double n1 = CornerContribution(ii + i1, jj + j1, kk + k1, x1, y1, z1);
        double n2 = CornerContribution(ii + i2, jj + j2, kk + k2, x2, y2, z2);
        double n3 = CornerContribution(ii + 1, jj + 1, kk + 1, x3, y3, z3);
        return (float)((n0 + n1 + n2 + n3) * 32.0);
    }

    // Computes one simplex corner's weighted gradient contribution.
    private double CornerContribution(int i, int j, int k, double x, double y, double z)
    {
        // Points outside the corner's influence radius contribute nothing.
        double influence = 0.6 - x * x - y * y - z * z;
        if (influence <= 0) return 0;
        influence *= influence;

        // Hash the lattice corner to choose a repeatable gradient direction.
        int gradientIndex = permutationTable[(i & 255) + permutationTable[(j & 255) + permutationTable[k & 255]]] % GradientDirections.Length;
        Vector3 gradient = GradientDirections[gradientIndex];
        double dot = gradient.x * x + gradient.y * y + gradient.z * z;
        return influence * influence * dot;
    }

    // Rounds down to the nearest integer, including negative coordinates.
    private static int FloorToInteger(double value)
    {
        return (int)System.Math.Floor(value);
    }
}
