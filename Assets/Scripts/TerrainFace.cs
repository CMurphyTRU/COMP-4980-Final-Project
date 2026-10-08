using UnityEngine;

public class TerrainFace
{
    //Mesh for each terrain face
    Mesh mesh;

    // how detailed each terrain face needs to be
    int resolution;

    //which way each face is facing
    Vector3 localUp;

    // other two axis based on localUp
    Vector3 axisA;
    Vector3 axisB;

    //Constructor for TerrainFace
    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        // swap the coordinates of localup.x, y, and z with localUp.y, z, and x respectively to give us the second axis
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);

        // Find a vector that is perpendicular to both localUp and axisA
        axisB = Vector3.Cross(localUp, axisA);
    }
    
    //
    public void ConstructMesh()
    {
        //Array of Vector3 to hold the vertices
        // Resolution is the number of vertices along a single edge
        // So resolution squared is the total number of vertices in the mesh
        Vector3[] vertices = new Vector3[resolution * resolution];

        // Number of faces is (resolution - 1) squared. 
        // Each face has two triangles and each triangle has 3 vertices so we multiply by 6
        int[] triangleIndices = new [(resolution - 1) * (resolution - 1) * 6];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // tells us how close to completion each loop is
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y -)
            }
        }
    }



   
   
}
