using UnityEngine;
using System.Collections.Generic;

public class DensityField
{
    private float[,,] densities;

    private int size;
    private int mazeSize;
    private float tunnelRadius;
    private float surfaceThickness;
    private bool showCutaway;
    private float tunnelCurvature;

    public DensityField(int meshResolution, int mazeSize, float tunnelRadius, float surfaceThickness, bool showCutaway, float tunnelCurvature)
    {
        size = meshResolution;
        this.mazeSize = mazeSize;
        this.tunnelRadius = tunnelRadius;
        this.surfaceThickness = surfaceThickness;
        this.showCutaway = showCutaway;
        this.tunnelCurvature = tunnelCurvature;

        densities = new float[size, size, size];
    }

    public void Generate(MazeGenerator3D maze)
    {
        /*List<Vector3> passages = new List<Vector3>();

        for (int x = 0; x < mazeSize; x++)
        {
            for (int y = 0; y < mazeSize; y++)
            {
                for (int z = 0; z < mazeSize; z++)
                {
                    if (maze.IsPassage(x, y, z))
                    {
                        passages.Add(new Vector3(x, y, z));
                    }
                }
            }
        }*/

        List<Vector3> passages = GenerateCurvedPassages(maze);

        Vector3 centre = Vector3.one * ((mazeSize - 1) / 2f);
        float radius = (mazeSize - 1) / 2f;

        float scale = (mazeSize - 1f) / (size - 1f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 position = new Vector3(x * scale, y * scale, z * scale);

                    float sphereDensity = radius - Vector3.Distance(position, centre);

                    float minDistance = float.MaxValue;

                    foreach (Vector3 passage in passages)
                    {
                        float distance = Vector3.Distance(position, passage);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                        }
                    }

                    float tunnelDensity = minDistance - tunnelRadius;

                    
                    //densities[x, y,z ] = sphereDensity;

                    float protectedRock = surfaceThickness - sphereDensity;
                    float carvedTunnel = Mathf.Max(tunnelDensity, protectedRock);
                    float finalDensity = Mathf.Min(sphereDensity, carvedTunnel);

                    if (showCutaway)
                    {
                        float cutDensity = centre.z - position.z;

                        finalDensity = Mathf.Min(finalDensity, cutDensity);
                    }

                    densities[x, y, z] = finalDensity;

                }
            }
        }
    }

    public float GetDensity(int x, int y, int z)
    {
        return densities[x, y, z];
    }

    private Vector3 BezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float inverse = 1f - t;

        return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
    }

    private List<Vector3> GenerateCurvedPassages(MazeGenerator3D maze)
    {
        List<Vector3> passages = new List<Vector3>();

        Vector3Int[] directions = 
        {
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.forward
        };

        for (int x = 0; x < mazeSize; x++)
        {
            for (int y = 0; y < mazeSize; y++)
            {
                for (int z = 0; z < mazeSize; z++)
                {
                    if (!maze.IsPassage(x, y, z))
                    {
                        continue;
                    }

                    Vector3Int current = new Vector3Int(x, y, z);

                    passages.Add((Vector3)current);

                    foreach (Vector3Int direction in directions)
                    {
                        Vector3Int next = current + direction;

                        if (next.x >= mazeSize || next.y >= mazeSize || next.z >= mazeSize)
                        {
                            continue;
                        }

                        if (!maze.IsPassage(next.x, next.y, next.z))
                        {
                            continue;
                        }

                        Vector3 start = (Vector3) current;
                        Vector3 end = (Vector3) next;

                        Vector3 midpoint = (start + end) * 0.5f;

                        Vector3 offset = Random.insideUnitSphere * tunnelCurvature;

                        Vector3 axis = ((Vector3)direction).normalized;
                        offset -= Vector3.Dot(offset, axis) * axis;

                        Vector3 control = midpoint + offset;

                        for (int i = 1; i < 8; i++)
                        {
                            float t = i / 8f;

                            passages.Add(BezierPoint(start, control, end, t));
                        }
                    }
                }
            }
        }
        return passages;
    }

    
}
