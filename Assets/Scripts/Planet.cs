using UnityEngine;

public class Planet : MonoBehaviour
{
    MeshFilter[] meshFilters;

    void initialize()
    {
        meshFilters = new MeshFilter[6];

        for (int i = 0; i < 6; i++)
        {
            GameObject meshObj = new GameObject("mesh");
            meshObj.transform.parent = transform;

            meshObj.AddComponent<MeshRenderer>();
            meshFilters[i] = meshObj.AddComponent<MeshFilter>();
        }
    }
}
