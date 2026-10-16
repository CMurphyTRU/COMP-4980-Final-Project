using UnityEngine;

public class ColorGenerator
{
    ColorSettings settings;

    public ColorGenerator(ColorSettings settings)
    {
        this.settings = settings;
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }
}
