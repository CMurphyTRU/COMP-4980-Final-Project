using UnityEngine;

public class MazeCell
{
    // whether or not the cell has been visited
    public bool visited = false;

    // four variables representing the four walls of each maze cell
    public bool northWall = true;
    public bool southWall = true;
    public bool eastWall = true;
    public bool westWall = true;
}
