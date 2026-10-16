using UnityEngine;

public class DiamondSquareFace : MonoBehaviour
{
    private Mesh mesh;
    private int resolution;
    private Vector3 localUp;
    private Vector3 axisA;
    private Vector3 axisB;

    private float heightMultiplier;
    

    private DiamondSquareGenerator heightGenerator;

    public DiamondSquareFace(Mesh mesh, int resolution, Vector3 localUp, DiamondSquareGenerator heightGenerator, float heightMultiplier)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.heightGenerator = heightGenerator;
        this.heightMultiplier = heightMultiplier;
        

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        Color[] colors = new Color[vertices.Length];

        int triangleIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;

                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2f * axisA + (percent.y - 0.5f) * 2f * axisB;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                float height = heightGenerator.GetHeight(x, y);

                vertices[i] = pointOnUnitSphere * (1f + height * heightMultiplier);

                colors[i] = GetTerrainColor(height);

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triangleIndex] = i;
                    triangles[triangleIndex + 1] = i + resolution + 1;
                    triangles[triangleIndex + 2] = i + resolution;

                    triangles[triangleIndex + 3] = i;
                    triangles[triangleIndex + 4] = i + 1;
                    triangles[triangleIndex + 5] = i + resolution + 1;

                    triangleIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    private Color GetTerrainColor(float height)
    {
        if (height < 0.30f)
        {
            return new Color(0.65f, 0.55f, 0.35f);
        }

        if (height < 0.45f)
        {
            return new Color(0.85f, 0.75f, 0.5f);
        }

        if (height < 0.70f)
        {
            return new Color(0.2f, 0.6f, 0.2f);
        }

        if (height < 0.85f)
        {
            return new Color(0.45f, 0.45f, 0.45f);
        }

        return Color.white;
    }
}
