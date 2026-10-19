using UnityEngine;

// Combines noise layers to determine the distance of each vertex from the planet center.
internal sealed class ShapeGenerator
{
    private ShapeSettings settings;
    private SimpleNoiseFilter[] noiseFilters;
    public MinMax ElevationMinMax { get; private set; }

    // Loads the shape settings, creates noise filters, and resets the elevation tracking.
    public void UpdateSettings(ShapeSettings newSettings)
    {
        settings = newSettings;
        noiseFilters = new SimpleNoiseFilter[settings.noiseLayers.Length];

        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = new SimpleNoiseFilter(settings.noiseLayers[i].noiseSettings.simpleNoiseSettings);
        }

        ElevationMinMax = new MinMax();
    }

    // Displaces a unit sphere point according to which terrain noise layers are enabled.
    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0f;
        float elevation = 0f;

        // The first noise layer can also act as a mask for subsequent layers.
        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }

        // Add secondary layers if enabled, optionally scaled by the first layer.
        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)
            {
                float mask = settings.noiseLayers[i].useFirstLayerAsMask ? firstLayerValue : 1f;
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
            }
        }

        // Convert the relative noise displacement to an actual distance from the center.
        elevation = settings.planetRadius * (1f + elevation);
        ElevationMinMax.AddValue(elevation);
        return pointOnUnitSphere * elevation;
    }
}
