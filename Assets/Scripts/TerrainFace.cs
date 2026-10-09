using UnityEngine;

public class TerrainFace
{
    //Each TerrainFace gets a mesh
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

        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                //calculate the index by taking the number of iterations of the inner loop and multiplying it by the number of iterations of the outer loop
                // we multiply y by the resolution because x iterates n times before y iterates again (with n being the resolution)
                int i = x + y * resolution;

                // tells us how close to completion each loop is
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                // calculates the 3D position of each vertex on a cube face and stores that position in the vertices array
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                vertices[i] = pointOnUnitCube;

                // ensures that we are not on the last row or column of the face
                if (x != resolution - 1 && y != resolution - 1)
                {
                    // add the indices of the two triangles in the current square of the face and increment by 6
                    triangleIndices[triIndex] = i;
                    triangleIndices[triIndex + 1] = i + resolution + 1;
                    triangleIndices[triIndex + 2] = i + resolution;

                    triangleIndices[triIndex + 3] = i;
                    triangleIndices[triIndex + 4] = i + 1;
                    triangleIndices[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        // Clears the current mesh. Without this line, if we reduced the resolution of the planet, we may end up with vertices remaining from the higher resolution version causing visual errors
        mesh.clear();

        // sets the vertices of the mesh to the vertices contained in the vertices array
        mesh.vertices = vertices;

        // assigns the triangle indices to the mesh
        mesh.triangles = triangleIndices;

        // calculates surface normals based on the triangles we set. Normals are used by unity's lighting system to determine how light interacts with surfaces
        mesh.RecalculateNormals();
    }



   
   
}
