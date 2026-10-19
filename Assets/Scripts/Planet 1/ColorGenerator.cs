using UnityEngine;

// Creates a gradient lookup texture and sends it and the elevation bounds to the material.
internal sealed class ColorGenerator
{
    // Number of sampled gradient colors stored in the one-pixel-high texture.
    private const int TextureResolution = 50;
    private ColorSettings settings;
    private Texture2D texture;

    // Stores color settings and allocates the lookup texture if needed.
    public void UpdateSettings(ColorSettings newSettings)
    {
        settings = newSettings;
        if (texture == null)
        {
            texture = new Texture2D(TextureResolution, 1);
        }
    }

    // Passes the minimum and maximum generated elevations to the shader.
    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    // Samples the configured gradient into a texture and assigns it to the material.
    public void UpdateColors()
    {
        Color[] colors = new Color[TextureResolution];
        for (int i = 0; i < TextureResolution; i++)
        {
            colors[i] = settings.gradient.Evaluate(i / (TextureResolution - 1f));
        }

        texture.SetPixels(colors);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }
}
