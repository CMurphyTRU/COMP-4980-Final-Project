using UnityEngine;

public class DiamondSquareFace : MonoBehaviour
{
    // the unity mesh that will be filled with points and triangles
    private Mesh mesh;
    
    // number of vertices along each side of the face
    private int resolution;

    // direction this cube face points outward
    private Vector3 localUp;

    // two directions used to move across the square face
    private Vector3 axisA;
    private Vector3 axisB;

    // controls how strongly the height values raise the surface
    private float heightMultiplier;
    
    // provides the generated terrain height at each grid position
    private DiamondSquareGenerator heightGenerator;

    public DiamondSquareFace(Mesh mesh, int resolution, Vector3 localUp, DiamondSquareGenerator heightGenerator, float heightMultiplier)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.heightGenerator = heightGenerator;
        this.heightMultiplier = heightMultiplier;
        
        // rotate the components of the outward direction to get a direction across the face
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);

        // use the cross product to get a second direction at right angles to the first
        axisB = Vector3.Cross(localUp, axisA);
    }

    // build the vertices, triangles, and colors for this planet face
    public void ConstructMesh()
    {
        // create space for very vertex in the square grid
        Vector3[] vertices = new Vector3[resolution * resolution];

        // each square becomes two triangles. each triangle needs three vertex indices, so six indices per square
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        // store one terrain color for each vertex
        Color[] colors = new Color[vertices.Length];

        // track the next free position in the triangle index array
        int triangleIndex = 0;

        // visit every row and column of the face grid
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // convert the 2D grid coordinates into a position in the vertex array
                int i = x + y * resolution;
                
                // convert grid coordinates to values from 0 to 1 across the face
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                
                // start at the centerof this cube face and move across it using the two face axes
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2f * axisA + (percent.y - 0.5f) * 2f * axisB;

                // normalize the cube point so it lies on a sphere of radius 1
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                // read the terrain height for this grid point
                float height = heightGenerator.GetHeight(x, y);

                // move the point outward from the sphere according to its terrain height
                vertices[i] = pointOnUnitSphere * (1f + height * heightMultiplier);

                // choose the terrain color based on the height
                colors[i] = GetTerrainColor(height);

                // only create trianges when a square exists to the right and above the point
                if (x != resolution - 1 && y != resolution - 1)
                {
                    // connect four neighbouring vertices into two triangles, their index order controls which way the surface faces
                    triangles[triangleIndex] = i;
                    triangles[triangleIndex + 1] = i + resolution + 1;
                    triangles[triangleIndex + 2] = i + resolution;

                    triangles[triangleIndex + 3] = i;
                    triangles[triangleIndex + 4] = i + 1;
                    triangles[triangleIndex + 5] = i + resolution + 1;

                    // move forward by two indices
                    triangleIndex += 6;
                }
            }
        }

        // replace old mesh data with the new vertices, triangles, and colors
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // recalculate the lighting normals to smooth it out
        mesh.RecalculateNormals();
    }

    // choose a color from the normalized height.
    private Color GetTerrainColor(float height)
    {
        // lowest land gets the color brown
        if (height < 0.30f)
        {
            return new Color(0.65f, 0.55f, 0.35f);
        }

        // low land gets the color sandy beige
        if (height < 0.45f)
        {
            return new Color(0.85f, 0.75f, 0.5f);
        }
        
        // middle elevations get the color green
        if (height < 0.70f)
        {
            return new Color(0.2f, 0.6f, 0.2f);
        }

        // high elevations get the color grey
        if (height < 0.85f)
        {
            return new Color(0.45f, 0.45f, 0.45f);
        }

        // highest points get the color white
        return Color.white;
    }
}
