using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator3D
{
    private int size;
    private MazeCell_3D[,,] cells;

    private Vector3Int[] directions = 
    {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.forward,
        Vector3Int.back
    };

    private float radius;
    private Vector3 centre;

    public MazeGenerator3D(int size)
    {
        this.size = size;
        
        centre = new Vector3(
            (size - 1) / 2f,
            (size - 1) / 2f,
            (size - 1) /2f
        );

        radius = (size - 1) / 2f - 1f;

        cells = new MazeCell_3D[size, size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    cells[x, y, z] = new MazeCell_3D();
                }
            }
        }
    }

    public void GenerateMaze()
    {
        Stack<Vector3Int> stack = new Stack<Vector3Int>();

        int middle = size / 2;

        if (middle % 2 == 0)
        {
            middle--;
        }

        Vector3Int start = new Vector3Int(
            middle,
            middle,
            middle
        );

        if (!IsValid(start))
        {
            Debug.LogError("Maze starting position is outside the sphere");
            return;
        }

        cells[start.x, start.y, start.z].visited = true;
        cells[start.x, start.y, start.z].isPassage = true;

        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector3Int current = stack.Peek();

            List<Vector3Int> neighbours = new List<Vector3Int>();

            foreach (Vector3Int direction in directions)
            {
                Vector3Int next = current + direction * 2;

                if (IsValid(next) && !cells[next.x, next.y, next.z].visited)
                {
                    neighbours.Add(next);
                }
            }

            if (neighbours.Count > 0)
            {
                Vector3Int next = neighbours[Random.Range(0, neighbours.Count)];

                Vector3Int between = (current + next) / 2;

                cells[between.x, between.y, between.z].isPassage = true;

                cells[next.x, next.y, next.z].visited = true;
                cells[next.x, next.y, next.z].isPassage = true;

                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    private bool IsValid(Vector3Int position)
    {
        if (position.x <= 0 || position.x >= size - 1|| position.y <= 0 || position.y >= size - 1|| position.z <= 0 || position.z >= size - 1)
        {
            return false;
        }

        float distance = Vector3.Distance(
            (Vector3)position,
            centre
        );

        return distance <= radius;
    }

    public bool IsPassage(int x, int y, int z)
    {
        return cells[z, y, z].isPassage;
    }
}
