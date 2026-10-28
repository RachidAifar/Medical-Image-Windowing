namespace ImageWindowing;

/// <summary>
/// Synthetic CT head slice in Hounsfield units, built from the Shepp-Logan ellipse geometry.
/// Used instead of real patient data, so the project contains no medical records.
/// </summary>
public static class HeadPhantom
{
    public const short Air = -1000;

    // x0, y0, semi-axis a, semi-axis b, rotation (degrees), HU added inside the ellipse.
    private static readonly (double X, double Y, double A, double B, double Angle, short Hu)[] Ellipses =
    [
        (0, 0, 0.69, 0.92, 0, 2000),           // skull: -1000 + 2000 = 1000 HU (bone)
        (0, -0.0184, 0.6624, 0.874, 0, -960),  // brain: 1000 - 960 = 40 HU
        (0.22, 0, 0.11, 0.31, -18, -40),       // right ventricle: 0 HU (fluid)
        (-0.22, 0, 0.16, 0.41, 18, -40),       // left ventricle: 0 HU (fluid)
        (0, 0.35, 0.21, 0.25, 0, 20),          // lesions and small structures: 60 HU
        (0, 0.1, 0.046, 0.046, 0, 20),
        (0, -0.1, 0.046, 0.046, 0, 20),
        (-0.08, -0.605, 0.046, 0.023, 0, 20),
        (0, -0.606, 0.023, 0.023, 0, 20),
        (0.06, -0.605, 0.023, 0.046, 0, 20),
    ];

    public static short[] Generate(int size)
    {
        if (size < 2)
            throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be at least 2.");

        var pixels = new short[size * size];
        for (var row = 0; row < size; row++)
        {
            // Image rows go top to bottom; phantom coordinates go bottom to top.
            var y = 1 - 2.0 * row / (size - 1);
            for (var col = 0; col < size; col++)
            {
                var x = 2.0 * col / (size - 1) - 1;
                pixels[row * size + col] = ValueAt(x, y);
            }
        }
        return pixels;
    }

    public static short ValueAt(double x, double y)
    {
        int value = Air;
        foreach (var e in Ellipses)
        {
            var radians = e.Angle * Math.PI / 180;
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            var dx = x - e.X;
            var dy = y - e.Y;
            var u = (dx * cos + dy * sin) / e.A;
            var v = (-dx * sin + dy * cos) / e.B;
            if (u * u + v * v <= 1)
                value += e.Hu;
        }
        return (short)value;
    }
}
