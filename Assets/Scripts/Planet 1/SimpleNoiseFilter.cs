using UnityEngine;

// Creates fractal terrain noise by combining several octaves of 3D simplex noise.
internal sealed class SimpleNoiseFilter
{
    private readonly NoiseSettings.SimpleNoiseSettings settings;
    private readonly Noise noise = new Noise();

    public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings settings)
    {
        this.settings = settings;
    }

    // Samples 3D simplex noise and returns a smoothly varying value, approximately between -1 and 1.
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0f;
        float frequency = settings.baseRoughness;
        float amplitude = 1f;

        // Each octave increases the frequency and reduces or changes the amplitude.
        for (int i = 0; i < settings.numLayers; i++)
        {
            float value = noise.Evaluate(point * frequency + settings.centre);

            // Remap approximately -1..1 noise into 0..1 before weighting it.
            noiseValue += (value + 1f) * 0.5f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        // Remove values below the floor, then scale the remaining elevation.
        noiseValue = Mathf.Max(0f, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
