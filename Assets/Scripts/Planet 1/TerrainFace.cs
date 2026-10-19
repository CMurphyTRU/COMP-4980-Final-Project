using UnityEngine;

// Creates one square mesh face, projects it onto a sphere, and applies terrain displacement.
internal sealed class TerrainFace
{
    // Helper objects that control terrain shape and surface colors.
    private readonly ShapeGenerator shapeGenerator;
    private readonly Mesh mesh;
    private readonly int resolution;
    private readonly Vector3 localUp;
    private readonly Vector3 axisA;
    private readonly Vector3 axisB;

    // Defines the orientation of the face and two perpendicular axes across its surface.
    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    // Generates the vertices and triangle indices for this face and recalculates the normals.
    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];

        // Every grid cell contains two triangles which requires six vertex indices.
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triangleIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int vertexIndex = x + y * resolution;

                // Convert grid coordinates into the 0-1 range for both face axes.
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                // Map the grid to a cube face, normalize it onto a sphere, then displace it.
                Vector3 pointOnCube = localUp + (percent.x - 0.5f) * 2f * axisA + (percent.y - 0.5f) * 2f * axisB;
                vertices[vertexIndex] = shapeGenerator.CalculatePointOnPlanet(pointOnCube.normalized);

                // Connect each grid cell to two triangles, except along the final row/column.
                if (x < resolution - 1 && y < resolution - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + resolution + 1;
                    triangles[triangleIndex + 2] = vertexIndex + resolution;
                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + 1;
                    triangles[triangleIndex + 5] = vertexIndex + resolution + 1;
                    triangleIndex += 6;
                }
            }
        }

        // Replace the old geometry and recalculate the lighting normals.
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
