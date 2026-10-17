using UnityEngine;

public class MazePlanet : MonoBehaviour
{
    [Header("Maze Settings")]
    [Range(5, 31)]
    public int mazeSize = 11;

    [Header("Debug Settings")]
    public bool showCutaway = false;

    [Header("Tunnel Settings")]
    [Range(0.5f, 1.5f)]
    public float tunnelRadius = 0.8f;
    [Range(0f, 1f)]
    public float tunnelCurvature = 0.4f;

    [Header("Mesh Settings")]
    [Range(11, 101)]
    public int meshResolution = 33;

    [Header("Random Generation")]
    public bool randomizeSeed = true;
    public int seed = 12345;

    [Header("Surface Settings")]
    [Range(0f, 3f)]
    public float surfaceThickness = 1.5f;

    public float cellSize = 1f;

    private MazeGenerator3D mazeGenerator;

    public Material planetMaterial;

    private DensityField densityField;

    void Start()
    {
        GeneratePlanet();
    }

    void GeneratePlanet()
    {
        if (randomizeSeed)
        {
            seed = System.Environment.TickCount;
        }

        Random.InitState(seed);

        mazeGenerator = new MazeGenerator3D(mazeSize);
        mazeGenerator.GenerateMaze();

        densityField = new DensityField(meshResolution, mazeSize, tunnelRadius, surfaceThickness, showCutaway, tunnelCurvature);
        densityField.Generate(mazeGenerator);

        MarchingCubesGenerator marchingCubes = new MarchingCubesGenerator(densityField, mazeSize, cellSize);

        marchingCubes.AnalyzeCubes();

        int middle = mazeSize / 2;

        Debug.Log("Centre density: " + densityField.GetDensity(middle, middle, middle));

        GenerateMesh();
    }

    
    void GenerateMesh()
    {
        MarchingCubesGenerator meshGenerator = new MarchingCubesGenerator(densityField, meshResolution, (mazeSize - 1f) * cellSize / (meshResolution - 1f));

        Mesh mesh = meshGenerator.GenerateMesh();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = planetMaterial;
    }
}
