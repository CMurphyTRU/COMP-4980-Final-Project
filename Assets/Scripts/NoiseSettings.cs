using UnityEngine;

[System.Serializable]
public class NoiseSettings
{

    public enum FilterType { Simple, Rigid};
    public FilterType filterType;
    public float strength = 1;

    [Range(1, 8)]
    public int numLayers = 1;
    public float baseRoughness = 1;

    public float roughness = 1;
    public float persistence = 0.5f;
    public Vector3 centre;

    public float minValue;
}
