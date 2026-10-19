// Tracks the smallest and largest elevations encountered during mesh generation.
internal sealed class MinMax
{
    public float Min { get; private set; } = float.MaxValue;
    public float Max { get; private set; } = float.MinValue;

    // Updates the stored elevation range with the newly generated value.
    public void AddValue(float value)
    {
        if (value > Max) Max = value;
        if (value < Min) Min = value;
    }
}
