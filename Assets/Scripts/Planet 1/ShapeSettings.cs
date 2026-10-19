using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    public float planetRadius = 1;
    public NoiseLayer[] noiseLayers;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask;
        public NoiseSettings noiseSettings;
    }
}

[System.Serializable]
public class NoiseSettings
{
    public SimpleNoiseSettings simpleNoiseSettings;

    [System.Serializable]
    public class SimpleNoiseSettings
    {
        public float strength = 1;

        [Range(1, 8)]
        public int numLayers = 1;
        public float baseRoughness = 1;
        public float roughness = 1;
        public float persistence = 0.5f;
        public Vector3 centre;
        public float minValue;
    }
}
