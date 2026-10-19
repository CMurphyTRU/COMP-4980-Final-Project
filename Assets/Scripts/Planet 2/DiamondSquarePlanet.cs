using UnityEngine;
using System.Collections.Generic;

// builds six square terrain faces, joins their edges, and adds an ocean
public class DiamondSquarePlanet : MonoBehaviour
{
    // controls the number of points along each side of a face.
    [Range(2, 257)]
    public int resolution = 33;

    [Range(0f, 1f)]
    public float heightMultiplier = 0.2f;

    // random seed for random generation
    [Header("Terrain Generation")]
    public int seed = 12345;

    // sets the height of the ocean surface relative to the terrain
    [Header("Ocean Settings")]
    [Range(0f, 1f)]
    public float seaLevel = 0.3f;

    public Material oceanMaterial;

    // controls how strongly the random height changes shrink at each step
    [Range(0f, 1f)]
    public float roughness = 0.5f;


    [SerializeField]
    private Material planetMaterial;

    // store the six meshes
    private MeshFilter[] meshFilters;

    // store the objects that build the geometry of each face
    private DiamondSquareFace[] terrainFaces;

    // stores a one height map generator for each face
    private DiamondSquareGenerator[] heightGenerators;

    // remembers generated edge heights so neighboring faces can have matching borders
    private Dictionary<int, float[]> sharedEdges;

    


    
    void Start()
    {
        GeneratePlanet();
    }

    // creates a new random layout, prepares the faces, makes their heights consistent then builds the land and ocean
    public void GeneratePlanet()
    {
        // use the current system tick count as a seed so we get new terrain each time play mode is entered
        seed = System.Environment.TickCount;
        Random.InitState(seed);
        
        Initialize();
        NormalizePlanetHeights();
        GenerateMesh();
        GenerateOcean();
    }

    // set up the six sides of a cube, generate the shared edge heights, and prepare each terrain mesh
    void Initialize()
    {
        // each direction points outward from one of the six faces of the cube
        Vector3[] directions = {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back,
        };

        // the cube has eighit corners, give each corner one height that all neighboring faces can share
        float[] cornerHeights = new float[8];

        for (int i = 0; i < cornerHeights.Length; i++)
        {
            cornerHeights[i] = Random.value;
        }

        // arrays to store the 6 meshes and face builders
        meshFilters = new MeshFilter[6];
        terrainFaces = new DiamondSquareFace[6];
        
        // create an empty lookup table of already generated border heights
        sharedEdges = new Dictionary<int, float[]>();

        // create the helper that makes a one-dimensional height sequence for each border
        DiamondSquareEdgeGenerator edgeGenerator = new DiamondSquareEdgeGenerator(resolution, roughness);

        // each face gets its own two-dimensional height map
        heightGenerators = new DiamondSquareGenerator[6];

        for (int i = 0; i < 6; i++)
        {
            // select which direction this face points outward
            Vector3 localUp = directions[i];

            // find a direction across this face that is perpendicular to its outward direction
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);

            // find the other direction across the face using the cross product
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            // find the four corners of this cube face before rounding it into a sphere
            Vector3 bottomLeft = localUp - axisA - axisB;
            Vector3 bottomRight = localUp + axisA - axisB;
            Vector3 topLeft = localUp - axisA + axisB;
            Vector3 topRight = localUp + axisA + axisB;

            // create a terrain height generator for the current face
            DiamondSquareGenerator heightGenerator = new DiamondSquareGenerator(resolution, roughness);

            // fill all four borders with shared heights bnefore generating the interior
            AssignEdge(heightGenerator, edgeGenerator, bottomLeft, bottomRight, 0, cornerHeights);
            AssignEdge(heightGenerator, edgeGenerator, bottomRight, topRight, 1, cornerHeights);
            AssignEdge(heightGenerator, edgeGenerator, topLeft, topRight, 2, cornerHeights);
            AssignEdge(heightGenerator, edgeGenerator, bottomLeft, topLeft, 3, cornerHeights);

            // run diamond-square for the interior using the four corner heights
            heightGenerator.GenerateHeights(cornerHeights[GetCornerIndex(bottomLeft)], cornerHeights[GetCornerIndex(bottomRight)], cornerHeights[GetCornerIndex(topLeft)], cornerHeights[GetCornerIndex(topRight)]);

            // save the height generator for later normalization
            heightGenerators[i] = heightGenerator;

            // create a child object to hold the mesh for this face
            GameObject meshObject = new GameObject("Terrain Face " + 1);

            // parent the face to the planet and reset its local position, rotation and scale
            meshObject.transform.parent = transform;
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localRotation = Quaternion.identity;
            meshObject.transform.localScale = Vector3.one;

            // add the component that displays the mesh and assign the planets material
            MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = planetMaterial;

            // add a meshfilter and give it a new empty mesh to fill later
            meshFilters[i] = meshObject.AddComponent<MeshFilter>();
            meshFilters[i].sharedMesh = new Mesh();

            // create the face builder and pass its mesh, direction, heightmap and terrain height multiplier
            terrainFaces[i] = new DiamondSquareFace(
                meshFilters[i].sharedMesh,
                resolution,
                directions[i],
                heightGenerator,
                heightMultiplier
            );
        }
    }

    // ask each face to construct its mesh, the blend the normals where the face borders meet
    void GenerateMesh()
    {
        foreach (DiamondSquareFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
        
        // smooth the lighting
        SmoothNormals();
    }
    
    // convert the signs of a cube corners X, Y, and Z values into one number from 0 to 7
    private int GetCornerIndex(Vector3 point)
    {
        // positive coordinates become 1 and negative coordinates become 0
        int x = point.x > 0 ? 1 : 0;
        int y = point.y > 0 ? 1 : 0;
        int z = point.z > 0 ? 1 : 0;

        // combine the three 0 or 1 values into a unique corner index
        return x * 4 + y * 2 + z;
    }
    
    // give an edge the same key no matter which of its two endpoints is listed first
    private int GetEdgeKey(int cornerA, int cornerB)
    {
        // sort the two corner indices so reversed edges have the same key
        int min = Mathf.Min(cornerA, cornerB);
        int max = Mathf.Max(cornerA, cornerB);

        // combine the two corner indices into a unique lookup key
        return min * 8 + max;
    }

    // copy shared border heights into a faces height map and mark those points as fixed
    private void AssignEdge(DiamondSquareGenerator generator, DiamondSquareEdgeGenerator edgeGenerator, Vector3 start, Vector3 end, int edgeNumber, float[] cornerHeights)
    {
        // find the unique indices of this edges two endpoints
        int startIndex = GetCornerIndex(start);
        int endIndex = GetCornerIndex(end);

        // look up the edge using its direction-independent key
        int key = GetEdgeKey(startIndex, endIndex);

        // if this edge has not been generated yet, make its heights and save them for other faces
        if (!sharedEdges.TryGetValue(key, out float[] heights))
        {
            int min = Mathf.Min(startIndex, endIndex);
            int max = Mathf.Max(startIndex, endIndex);

            // generate the border from the two corner heights
            heights = edgeGenerator.GenerateEdge(cornerHeights[min], cornerHeights[max]);

            sharedEdges.Add(key, heights);
        }

        // check whether this face travels along the shared edge in the opposite direction
        bool reversed = startIndex > endIndex;

        for (int i = 0; i < resolution; i++)
        {
            // read the heights backwards if needed so both faces match point for point
            float height = reversed ? heights[resolution - 1 - i] : heights[i];

            int x = 0;
            int y = 0;

            // edge 0 is bottom, 1 is right, 2 is top, and 3 is left
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

            // save the edge height and prevent Diamond-Square from replacing it
            generator.SetFixedHeight(x, y, height);
        }


    }

    // average the surface normals of vertices at the same position to hide the lighting seams between faces
    void SmoothNormals()
    {   
        // store the total normal direction for each matching vertex position
        Dictionary<Vector3, Vector3> normalSums = new Dictionary<Vector3, Vector3>();

        // read every face mesh and add each vertex normal to the total at that position
        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
            {
                // use the vertex position as the key for finding matching points on other faces
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
                // normalize the summed direction so it can be used a surface normal
                normals[i] = normalSums[vertices[i]].normalized;
            }

            // save the smoothed normals back into the mesh
            mesh.normals = normals;
        }
    }

    // find the lowest and highest heights anywhere on the planet, the scale every face using that same range
    void NormalizePlanetHeights()
    {
        // begin with extreme values so the first real height updates both limits
        float globalMin = float.MaxValue;
        float globalMax = float.MinValue;

        foreach (DiamondSquareGenerator generator in heightGenerators)
        {
            // ask this face for its smallest and largest terrain heights
            generator.GetHeightRange(out float min, out float max);

            // update the lowest and highest heights across all six faces
            globalMin = Mathf.Min(globalMin, min);
            globalMax = Mathf.Max(globalMax, max);
        }

        foreach (DiamondSquareGenerator generator in heightGenerators)
        {
            // scale this faces heights with the shared planet-wide limits
            generator.NormalizeHeights(globalMin, globalMax);
        }
    }

    // create a sphere inside the planet to make lower terrain appear submerged
    void GenerateOcean()
    {
        // create a standard unity sphere
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        ocean.name = "Ocean";

        // make sure the ocean is centered relative to the planet
        ocean.transform.SetParent(transform, false);

        // calculate the ocean radius using the sea level and height multiplier variable
        float oceanRadius = 1f + seaLevel * heightMultiplier;

        ocean.transform.localScale = Vector3.one * oceanRadius * 2f;

        MeshRenderer renderer = ocean.GetComponent<MeshRenderer>();

        // apply the ocean material
        renderer.sharedMaterial = oceanMaterial;

        // remove the ocean collider
        Destroy(ocean.GetComponent<Collider>());
    }

}
