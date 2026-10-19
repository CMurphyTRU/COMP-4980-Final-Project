using UnityEngine;


public class Planet : MonoBehaviour
{
    //Controls the number of vertices along each edge of a cube face. higher values make smoother terrain
    [Range(2, 256)]
    public int resolution = 10;


    // controls whether or not the planet should be automatically rebuilt when planet settings are changed
    public bool autoUpdate = true;

    // allows a single face to be displayed when inspecting the generated mesh
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
    public FaceRenderMask faceRenderMask;

    // settings to control terrain elevation and terrain color
    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    // editor foldout code, not related to generation
    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colorSettingsFoldout;

    // controls whether or not the planet should be randomly generated each time play mode is entered
    [Header("Random Generation")]
    public bool randomizeSeed = true;
    public int seed = 12345;

    // helper objects for controlling terrain shape and surface colors
    private readonly ShapeGenerator shapeGenerator = new ShapeGenerator();
    private readonly ColorGenerator colorGenerator = new ColorGenerator();

    // Arrays to hold the meshfilters and terrainfaces
    [SerializeField, HideInInspector]
    private MeshFilter[] meshFilters;
    private TerrainFace[] terrainFaces;

    // builds the planet
    private void Start()
    {
        RandomizeTerrain();

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        GeneratePlanet();
        stopwatch.Stop();

        Debug.Log($"Simplex noise generation time: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
    }

    // prepares the generators and either creates or reuses the six cube-face meshes based on if meshfilters is empty or not
    private void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings);

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }

        terrainFaces = new TerrainFace[6];

        // array containing the six directions of the faces of the original cube
        Vector3[] directions =
        {
            Vector3.up, Vector3.down, Vector3.left,
            Vector3.right, Vector3.forward, Vector3.back
        };


        for (int i = 0; i < directions.Length; i++)
        {
            // creates the meshes if they do not already exist
            if (meshFilters[i] == null)
            {
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.parent = transform;
                meshObject.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;
            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);

            // only renders the selected face in the inspector
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    // builds or rebuilds the planet geometry and updates its material colors
    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColors();
    }

    // if auto update is enabled, rebuilds the terrain when the shape settings are changed
    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
        }
    }

    // if auto update is enabled, refreshes the material colors.
    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateColors();
        }
    }

    // constructs the face meshes and passes the bounds of the elevations to the color generator
    private void GenerateMesh()
    {
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }

        colorGenerator.UpdateElevation(shapeGenerator.ElevationMinMax);
    }

    // builds the gradient used by the planet material
    private void GenerateColors()
    {
        colorGenerator.UpdateColors();
    }

    // sets either a random or a fixed seed assigned each noise layer a 3d sampling offset
    private void RandomizeTerrain()
    {
        if (randomizeSeed)
        {
            seed = System.Environment.TickCount;
        }

        // call unity's random generator so the offsets cand be reproduced
        Random.InitState(seed);

        foreach (ShapeSettings.NoiseLayer layer in shapeSettings.noiseLayers)
        {
            layer.noiseSettings.simpleNoiseSettings.centre = new Vector3(
                Random.Range(-10000f, 10000f),
                Random.Range(-10000f, 10000f),
                Random.Range(-10000f, 10000f));
        }
    }
}
