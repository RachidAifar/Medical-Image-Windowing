namespace ImageWindowing;

/// <summary>
/// A display window defined by its center (level) and width, in Hounsfield units (HU).
/// Follows the DICOM VOI LUT definition (PS3.3 C.11.2.1.2), where width must be at least 1.
/// </summary>
public readonly record struct WindowLevel
{
    public double Center { get; }
    public double Width { get; }

    public WindowLevel(double center, double width)
    {
        if (double.IsNaN(center) || double.IsInfinity(center))
            throw new ArgumentOutOfRangeException(nameof(center), center, "Center must be a finite number.");
        if (double.IsNaN(width) || width < 1)
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be at least 1.");

        Center = center;
        Width = width;
    }

    /// <summary>Lowest value that is still shown as black.</summary>
    public double Lower => Center - 0.5 - (Width - 1) / 2;

    /// <summary>Highest value that is still below full white.</summary>
    public double Upper => Center - 0.5 + (Width - 1) / 2;

    /// <summary>Builds a window that spans the given value range exactly.</summary>
    public static WindowLevel FromRange(double min, double max)
    {
        if (max < min)
            throw new ArgumentException("max must be greater than or equal to min.", nameof(max));

        var width = Math.Max(1, max - min + 1);
        return new WindowLevel(min + width / 2, width);
    }

    public override string ToString() => $"C={Center:0.##} W={Width:0.##}";
}
