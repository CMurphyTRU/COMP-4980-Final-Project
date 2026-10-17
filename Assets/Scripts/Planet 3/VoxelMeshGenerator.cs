using UnityEngine;
using System.Collections.Generic;

public class VoxelMeshGenerator
{
    private MazeGenerator3D maze;
    private int size;
    private float cellSize;
    private bool showCutaway;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private Vector3Int[] directions = 
    {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.forward,
        Vector3Int.back
    };

    public VoxelMeshGenerator(MazeGenerator3D maze, int size, float cellSize, bool showCutaway)
    {
        this.maze = maze;
        this.size = size;
        this.cellSize = cellSize;
        this.showCutaway = showCutaway;
    }

    public Mesh GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (!IsSolid(x, y, z))
                    {
                        continue;
                    }

                    Vector3Int position = new Vector3Int(x, y, z);

                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int neighbour = position + directions[face];

                        if (!IsSolid(neighbour.x, neighbour.y, neighbour.z))
                        {
                            AddFace(position, face);
                        }
                    }
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

    private bool IsSolid(int x, int y, int z)
    {
        if (x < 0 || x >= size || y < 0 || y >= size || z < 0 || z >= size)
        {
            return false;
        }

        Vector3 centre = Vector3.one * ((size - 1) / 2f);
        Vector3 position = new Vector3(x, y, z);

        float radius = (size - 1) / 2f;

        if (Vector3.Distance(position, centre) > radius)
        {
            return false;
        }

        if (showCutaway && z > centre.z)
        {
            return false;
        }

        return !maze.IsPassage(x, y, z);
    }

    private void AddFace(Vector3Int cell, int face)
    {
        float offset = (size - 1) * cellSize / 2f;

        Vector3 centre = new Vector3(cell.x * cellSize - offset, cell.y * cellSize - offset, cell.z * cellSize - offset);

        Vector3 normal = (Vector3)directions[face];

        //Vector3 axisA = new Vector3(normal.y, normal.z, normal.x);
        Vector3 axisA = Vector3.Cross(normal, normal == Vector3.up || normal == Vector3.down ? Vector3.forward : Vector3.up).normalized;

        Vector3 axisB = Vector3.Cross(normal, axisA);

        int startIndex = vertices.Count;

        vertices.Add(centre + (normal - axisA - axisB) * cellSize * 0.5f);
        vertices.Add(centre + (normal + axisA - axisB) * cellSize * 0.5f);
        vertices.Add(centre + (normal + axisA + axisB) * cellSize * 0.5f);
        vertices.Add(centre + (normal - axisA + axisB) * cellSize * 0.5f);

        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);

        triangles.Add(startIndex);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 3);

    }
}
