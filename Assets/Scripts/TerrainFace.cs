using UnityEngine;

public class TerrainFace
{
    //Mesh for each terrain face
    Mesh mesh;

    // how detailed each terrain face needs to be
    int resolution;

    //which way each face is facing
    Vector3 localUp;

    //Constructor for TerrainFace
    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
    }

   
   
}
