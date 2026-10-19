using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    // settings to control how many maze cells there are in the maze
    [Header("Maze Settings")]
    public int width = 10;
    public int height =  10;

    // variables to control the size of the cells, and the height and thickness of the walls.
    [Header("Visual Settings")]
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.2f;

    // two dimensional array that stores the maze cells
    private MazeCell[,] grid;

    void Start()
    {
        InitializeGrid();
        GenerateMaze();
        CreateEntranceAndExit();
        GenerateFloor();
        GenerateWalls();
    }

    // creates the two dimensional grid of maze cells
    void InitializeGrid()
    {   
        // creates an array using the variable-controlled width and height
        grid = new MazeCell[width, height];
        
        // loops through every position on the grid
        // the outer loop moves through the x axis of the grid
        // the inner loop moves through the y axis of the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // fill the array with new mazecells
                grid[x, y] = new MazeCell();
            }
        }
    }

    // generates the maze using the Depth-First search algorithm
    void GenerateMaze()
    {
        // create a stack to remember the cells along the current path
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // start at the bottom-left cell
        Vector2Int start = new Vector2Int(0, 0);

        // marks the initial cell as visited and adds the starting position to the stack
        grid[start.x, start.y].visited = true;
        stack.Push(start);

        // continue exploring until the stack is empty
        // when the stack is empty, that means there are no more cells to explore
        while (stack.Count > 0)
        {
            // peek at the most recently added cell to the stack
            Vector2Int current = stack.Peek();

            // find neighbouring cells that havent been visited
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            // if at least one unvisited neighbour still exists, the algorithm can continue
            if (neighbors.Count > 0)
            {
                // randomly choose an available neighbour
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];

                // remove the walls that separate the current cell and the selected neighbouring cell
                RemoveWalls(current, next);

                // marks the selected neighbour as visited
                grid[next.x, next.y].visited = true;

                // adds the new cell to the stack
                stack.Push(next);
            }
            else
            {
                // if there are no unvisited neighbours, backtrack
                stack.Pop();
            }
        }

        Debug.Log("Maze generation complete");
    }

    // finds all the unvisited neighbours of the current cell
    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int current)
    {   
        // create an empty list to store valid neighbours
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // defines the four directions the algorithm can move
        Vector2Int[] directions = 
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        // check each of the four possible directions
        foreach (Vector2Int direction in directions)
        {   
            // calculate the coordinates of the neighboring cell by adding the direction to the current position
            Vector2Int neighbor = current + direction;

            // checks that the neighbour is inside the grid
            // neighbor.x >=: not outside the left edge
            // neighbor.x < width: not outside the right edge
            // neighbor.y >= 0: not outside the bottom edge
            // neighbor.y < height: not outside the top edge
            // the last condition checks that the neighbor has not already been visited
            if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height && !grid[neighbor.x, neighbor.y].visited)
            {
                // add the neighbor to the list of cells that the algorithm is allowed to visit
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    // removes the wall between two neighbouring cells
    void RemoveWalls(Vector2Int current, Vector2Int next)
    {
        // subtract the current position from the next position to determine which direction the algorithm moved
        Vector2Int difference = next - current;

        // if the next cell is above the current cell, remove the current cells north wall and the next cells south wall
        if (difference == Vector2Int.up)
        {
            grid[current.x, current.y].northWall = false;
            grid[next.x, next.y].southWall = false;
        }
        
        // if the next cell is below the current cell, remove the current cells south wall and the next cells north wall
        else if (difference == Vector2Int.down)
        {
            grid[current.x, current.y].southWall = false;
            grid[next.x, next.y].northWall = false;
        }
        
        // if the next cell is to the right, remove the current cells east wall and the next cells west wall
        else if (difference == Vector2Int.right)
        {
            grid[current.x, current.y].eastWall = false;
            grid[next.x, next.y].westWall = false;
        }

        // if the next cell is to the left, remove the current cells west wall and the next cells east wall
        else if (difference == Vector2Int.left)
        {
            grid[current.x, current.y].westWall = false;
            grid[next.x, next.y].eastWall = false;
        }
    }

    // converts the generated maze layour into physical 3d walls
    void GenerateWalls()
    {   
        // loop through every cell in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // get the current mazecell
                MazeCell cell  = grid[x, y];

                // calculate the cells position inside unity
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                // if the north wall is still present, create a wall along the positive z edge
                if (cell.northWall)
                {
                    CreateWall(cellPosition + new Vector3(0, 0, cellSize / 2), false);
                }
                
                // if the east wall is still present, create a wall along the positive x edge
                if (cell.eastWall)
                {
                    CreateWall(cellPosition + new Vector3(cellSize / 2, 0, 0), true);
                }

                // only create south walls for cells on the bottom boundary of the maze
                if (y == 0 && cell.southWall)
                {
                    CreateWall(cellPosition + new Vector3(0, 0, -cellSize / 2), false);
                }

                // onyl create west walls for cells on the left boundary of the maze
                if (x == 0 && cell.westWall)
                {
                    CreateWall(cellPosition + new Vector3(-cellSize / 2, 0, 0), true);
                }
            }
        }
    }
    
    // create a single physical wall using a unity cube
    void CreateWall(Vector3 position, bool vertical)
    {
        // create a new cube GameObject
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // name the wall 
        wall.name = "Maze Wall";

        // make the wall a child of the MazeGenerator
        wall.transform.parent = transform;

        // position the wall at the requested location
        wall.transform.localPosition = position + new Vector3(0, wallHeight / 2f, 0);

        // if vertical is true, create a wall extending along the z axis
        if (vertical)
        {
            wall.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize);
        }
        
        // otherwise create a wall extending along the x axis
        else
        {
            wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
        }
    }

    // creates a single large floor underneath the entire maze
    void GenerateFloor()
    {
        // create a cube to use as the maze floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // name the floor and make it a child of the maze object
        floor.name = "Maze Floor";
        floor.transform.parent = transform;

        // calculate the center position of the entire maze
        floor.transform.localPosition = new Vector3((width - 1) * cellSize / 2f, -0.1f, (height - 1) * cellSize / 2f);

        // resize the cube so it covers the entire mazez
        floor.transform.localScale = new Vector3(width * cellSize, 0.2f, height * cellSize);
    }

    // creates an entrance and an exit on the opposite sides of the maze
    void CreateEntranceAndExit()
    {
        grid[0, 0].southWall = false;

        grid[width - 1, height - 1].northWall = false;
    }
}
