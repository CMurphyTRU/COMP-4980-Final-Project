using UnityEngine;
using System.Collections.Generic;


public class DiamondSquarePlanet : MonoBehaviour
{
    [Range(2, 257)]
    public int resolution = 33;

    [Range(0f, 1f)]
    public float heightMultiplier = 0.2f;

    [Header("Terrain Generation")]
    public int seed = 12345;

    [Header("Ocean Settings")]
    [Range(0f, 1f)]
    public float seaLevel = 0.3f;

    public Material oceanMaterial;

    [Range(0f, 1f)]
    public float roughness = 0.5f;

    [SerializeField]
    private Material planetMaterial;

    private MeshFilter[] meshFilters;
    private DiamondSquareFace[] terrainFaces;
    private DiamondSquareGenerator[] heightGenerators;

    private Dictionary<int, float[]> sharedEdges;

    


    
    void Start()
    {
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        seed = System.Environment.TickCount;
        Random.InitState(seed);
        
        Initialize();
        NormalizePlanetHeights();
        GenerateMesh();
        GenerateOcean();
    }

    void Initialize()
    {
        Vector3[] directions = {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back,
        };

        float[] cornerHeights = new float[8];

        for (int i = 0; i < cornerHeights.Length; i++)
        {
            cornerHeights[i] = Random.value;
        }

        meshFilters = new MeshFilter[6];
        terrainFaces = new DiamondSquareFace[6];
        

        sharedEdges = new Dictionary<int, float[]>();

        DiamondSquareEdgeGenerator edgeGenerator = new DiamondSquareEdgeGenerator(resolution, roughness);

        heightGenerators = new DiamondSquareGenerator[6];

        for (int i = 0; i < 6; i++)
        {
            Vector3 localUp = directions[i];
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);

            Vector3 axisB = Vector3.Cross(localUp, axisA);

            Vector3 bottomLeft = localUp - axisA - axisB;
            Vector3 bottomRight = localUp + axisA - axisB;
            Vector3 topLeft = localUp - axisA + axisB;
            Vector3 topRight = localUp + axisA + axisB;

            DiamondSquareGenerator heightGenerator = new DiamondSquareGenerator(resolution, roughness);

            AssignEdge(heightGenerator, edgeGenerator, bottomLeft, bottomRight, 0, cornerHeights);
            AssignEdge(heightGenerator, edgeGenerator, bottomRight, topRight, 1, cornerHeights);
            AssignEdge(heightGenerator, edgeGenerator, topLeft, topRight, 2, cornerHeights);
            AssignEdge(heightGenerator, edgeGenerator, bottomLeft, topLeft, 3, cornerHeights);

            heightGenerator.GenerateHeights(cornerHeights[GetCornerIndex(bottomLeft)], cornerHeights[GetCornerIndex(bottomRight)], cornerHeights[GetCornerIndex(topLeft)], cornerHeights[GetCornerIndex(topRight)]);

            heightGenerators[i] = heightGenerator;


            GameObject meshObject = new GameObject("Terrain Face " + 1);

            meshObject.transform.parent = transform;
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localRotation = Quaternion.identity;
            meshObject.transform.localScale = Vector3.one;

            MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();

            renderer.sharedMaterial = planetMaterial;

            meshFilters[i] = meshObject.AddComponent<MeshFilter>();

            meshFilters[i].sharedMesh = new Mesh();

            terrainFaces[i] = new DiamondSquareFace(
                meshFilters[i].sharedMesh,
                resolution,
                directions[i],
                heightGenerator,
                heightMultiplier
            );
        }
    }

    void GenerateMesh()
    {
        foreach (DiamondSquareFace face in terrainFaces)
        {
            face.ConstructMesh();
        }

        SmoothNormals();
    }

    private int GetCornerIndex(Vector3 point)
    {
        int x = point.x > 0 ? 1 : 0;
        int y = point.y > 0 ? 1 : 0;
        int z = point.z > 0 ? 1 : 0;

        return x * 4 + y * 2 + z;
    }

    private int GetEdgeKey(int cornerA, int cornerB)
    {
        int min = Mathf.Min(cornerA, cornerB);
        int max = Mathf.Max(cornerA, cornerB);

        return min * 8 + max;
    }

    private void AssignEdge(DiamondSquareGenerator generator, DiamondSquareEdgeGenerator edgeGenerator, Vector3 start, Vector3 end, int edgeNumber, float[] cornerHeights)
    {
        int startIndex = GetCornerIndex(start);
        int endIndex = GetCornerIndex(end);

        int key = GetEdgeKey(startIndex, endIndex);

        if (!sharedEdges.TryGetValue(key, out float[] heights))
        {
            int min = Mathf.Min(startIndex, endIndex);
            int max = Mathf.Max(startIndex, endIndex);

            heights = edgeGenerator.GenerateEdge(cornerHeights[min], cornerHeights[max]);

            sharedEdges.Add(key, heights);
        }

        bool reversed = startIndex > endIndex;

        for (int i = 0; i < resolution; i++)
        {
            float height = reversed ? heights[resolution - 1 - i] : heights[i];

            int x = 0;
            int y = 0;

            switch (edgeNumber)
            {
                case 0:
                    x = i;
                    y = 0;
                    break;
                
                case 1:
                    x = resolution - 1;
                    y = i;
                    break;

                case 2:
                    x = i;
                    y = resolution - 1;
                    break;
                
                case 3:
                    x = 0;
                    y = i;
                    break;
            }

            generator.SetFixedHeight(x, y, height);
        }


    }

    void SmoothNormals()
    {
        Dictionary<Vector3, Vector3> normalSums = new Dictionary<Vector3, Vector3>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 position = vertices[i];

                if (normalSums.ContainsKey(position))
                {
                    normalSums[position] += normals[i];
                }
                else
                {
                    normalSums.Add(position, normals[i]);
                }
            }
        }

        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = normalSums[vertices[i]].normalized;
            }

            mesh.normals = normals;
        }
    }

    void NormalizePlanetHeights()
    {
        float globalMin = float.MaxValue;
        float globalMax = float.MinValue;

        foreach (DiamondSquareGenerator generator in heightGenerators)
        {
            generator.GetHeightRange(out float min, out float max);

            globalMin = Mathf.Min(globalMin, min);
            globalMax = Mathf.Max(globalMax, max);
        }

        foreach (DiamondSquareGenerator generator in heightGenerators)
        {
            generator.NormalizeHeights(globalMin, globalMax);
        }
    }

    void GenerateOcean()
    {
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        ocean.name = "Ocean";
        ocean.transform.SetParent(transform, false);

        float oceanRadius = 1f + seaLevel * heightMultiplier;

        ocean.transform.localScale = Vector3.one * oceanRadius * 2f;

        MeshRenderer renderer = ocean.GetComponent<MeshRenderer>();

        renderer.sharedMaterial = oceanMaterial;

        Destroy(ocean.GetComponent<Collider>());
    }

}
