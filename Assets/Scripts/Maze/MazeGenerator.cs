using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 10;
    public int height =  10;

    [Header("Visual Settings")]
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.2f;

    private MazeCell[,] grid;

    void Start()
    {
        InitializeGrid();
        GenerateMaze();
        CreateEntranceAndExit();
        GenerateFloor();
        GenerateWalls();
    }

    void InitializeGrid()
    {
        grid = new MazeCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new MazeCell();
            }
        }

        Debug.Log($"Initialized maze with {width * height} cells");
    }

    void GenerateMaze()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        Vector2Int start = new Vector2Int(0, 0);

        grid[start.x, start.y].visited = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();

            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];

                RemoveWalls(current, next);
                grid[next.x, next.y].visited = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        Debug.Log("Maze generation complete");
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int current)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = 
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = current + direction;

            if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height && !grid[neighbor.x, neighbor.y].visited)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    void RemoveWalls(Vector2Int current, Vector2Int next)
    {
        Vector2Int difference = next - current;

        if (difference == Vector2Int.up)
        {
            grid[current.x, current.y].northWall = false;
            grid[next.x, next.y].southWall = false;
        }
        else if (difference == Vector2Int.down)
        {
            grid[current.x, current.y].southWall = false;
            grid[next.x, next.y].northWall = false;
        }
        else if (difference == Vector2Int.right)
        {
            grid[current.x, current.y].eastWall = false;
            grid[next.x, next.y].westWall = false;
        }
        else if (difference == Vector2Int.left)
        {
            grid[current.x, current.y].westWall = false;
            grid[next.x, next.y].eastWall = false;
        }
    }

    void GenerateWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MazeCell cell  = grid[x, y];

                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                if (cell.northWall)
                {
                    CreateWall(cellPosition + new Vector3(0, 0, cellSize / 2), false);
                }

                if (cell.eastWall)
                {
                    CreateWall(cellPosition + new Vector3(cellSize / 2, 0, 0), true);
                }

                if (y == 0 && cell.southWall)
                {
                    CreateWall(cellPosition + new Vector3(0, 0, -cellSize / 2), false);
                }

                if (x == 0 && cell.westWall)
                {
                    CreateWall(cellPosition + new Vector3(-cellSize / 2, 0, 0), true);
                }
            }
        }
    }

    void CreateWall(Vector3 position, bool vertical)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

        wall.name = "Maze Wall";
        wall.transform.parent = transform;

        wall.transform.localPosition = position + new Vector3(0, wallHeight / 2f, 0);

        if (vertical)
        {
            wall.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize);
        }
        else
        {
            wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
        }
    }

    void GenerateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);

        floor.name = "Maze Floor";
        floor.transform.parent = transform;

        floor.transform.localPosition = new Vector3((width - 1) * cellSize / 2f, -0.1f, (height - 1) * cellSize / 2f);

        floor.transform.localScale = new Vector3(width * cellSize, 0.2f, height * cellSize);
    }

    void CreateEntranceAndExit()
    {
        grid[0, 0].southWall = false;

        grid[width - 1, height - 1].northWall = false;
    }
}
