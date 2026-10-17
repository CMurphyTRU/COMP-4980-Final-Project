using UnityEngine;
using System.Collections.Generic;

public class MarchingCubesGenerator
{
    private DensityField densityField;
    private int size;
    private float cellSize;

    private static readonly Vector3Int[] cornerOffsets =
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),

        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1),
    };

    private static readonly int[,] edgeConnections =
    {
        { 0, 1 },
        { 1, 2 },
        { 2, 3 },
        { 3, 0 },

        { 4, 5 },
        { 5, 6 },
        { 6, 7 },
        { 7, 4 },

        { 0, 4 },
        { 1, 5 },
        { 2, 6 },
        { 3, 7 },
    };

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    public MarchingCubesGenerator(DensityField densityField, int size, float cellSize)
    {
        this.densityField = densityField;
        this.size = size;
        this.cellSize = cellSize;
    }

    private int GetCubeIndex(int x, int y, int z)
    {
        int cubeIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = new Vector3Int(x, y, z) + cornerOffsets[i];

            float density = densityField.GetDensity(corner.x, corner.y, corner.z);

            if (density > 0f)
            {
                cubeIndex |= 1 << i;
            }
        }

        return cubeIndex;
    }

    public void AnalyzeCubes()
    {
        int surfaceCubes = 0;

        for (int x = 0; x < size - 1; x++)
        {
            for (int y = 0; y < size - 1; y++)
            {
                for (int z = 0; z < size - 1; z++)
                {
                    int cubeIndex = GetCubeIndex(x, y, z);

                    if (cubeIndex != 0 && cubeIndex != 255)
                    {
                        surfaceCubes++;

                        Vector3[] edgeVertices = GetEdgeVertices(x, y, z);

                        if (surfaceCubes == 1)
                        {
                            Debug.Log("First surface cube edge 0: " + edgeVertices[0]);
                        }
                    }
                }
            }
        }

        Debug.Log("Surface cubes found: " + surfaceCubes);
    }

    private Vector3 InterpolateVertex(Vector3 pointA, Vector3 pointB, float densityA, float densityB)
    {
        float difference = densityB - densityA;

        if (Mathf.Abs(difference) < 0.00001f)
        {
            return (pointA + pointB) * 0.5f;
        }

        float t = -densityA / difference;

        return Vector3.Lerp(pointA, pointB, t);
    }

    private Vector3[] GetEdgeVertices(int x, int y, int z)
    {
        Vector3[] cornerPositions = new Vector3[8];
        float[] cornerDensities = new float[8];

        Vector3 offset = Vector3.one * ((size - 1) * cellSize / 2f);

        for (int i = 0; i < 8; i++)
        {
            Vector3Int gridPosition = new Vector3Int(x, y, z) + cornerOffsets[i];

            cornerPositions[i] = (Vector3)gridPosition * cellSize - offset;

            cornerDensities[i] = densityField.GetDensity(gridPosition.x, gridPosition.y, gridPosition.z);
        }

        Vector3[] edgeVertices = new Vector3[12];

        for (int edge = 0; edge < 12; edge++)
        {
            int cornerA = edgeConnections[edge, 0];
            int cornerB = edgeConnections[edge, 1];

            float densityA = cornerDensities[cornerA];
            float densityB = cornerDensities[cornerB];

            if ((densityA > 0f) != (densityB > 0f))
            {
                edgeVertices[edge] = InterpolateVertex(cornerPositions[cornerA], cornerPositions[cornerB], densityA, densityB);
            }
        }

        return edgeVertices;
    }

    private void TriangulateCube(int x, int y, int z)
    {
        int cubeIndex = GetCubeIndex(x, y, z);

        if (cubeIndex == 0 || cubeIndex == 255)
        {
            return;
        }

        Vector3[] edgeVertices = GetEdgeVertices(x, y, z);

        for (int i = 0; i < 16; i += 3)
        {
            int edgeA = MarchingCubesTables.Triangles[cubeIndex, i];

            if (edgeA == -1)
            {
                break;
            }

            int edgeB = MarchingCubesTables.Triangles[cubeIndex, i + 1];
            int edgeC = MarchingCubesTables.Triangles[cubeIndex, i + 2];

            int startIndex = vertices.Count;

            vertices.Add(edgeVertices[edgeA]);
            vertices.Add(edgeVertices[edgeB]);
            vertices.Add(edgeVertices[edgeC]);

            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
        }
    }

    public Mesh GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        for (int x = 0; x < size - 1; x++)
        {
            for (int y = 0; y < size - 1; y++)
            {
                for (int z = 0; z < size - 1; z++)
                {
                    TriangulateCube(x, y, z);
                }
            }
        }

        Mesh mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
