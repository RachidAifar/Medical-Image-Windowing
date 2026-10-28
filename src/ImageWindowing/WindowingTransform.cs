namespace ImageWindowing;

/// <summary>
/// Maps Hounsfield values to 8-bit display values with the DICOM linear VOI function.
/// </summary>
public static class WindowingTransform
{
    public const byte OutputMin = 0;
    public const byte OutputMax = 255;

    /// <summary>Maps a single value. Rounds half up so the result is identical in every language.</summary>
    public static byte ToDisplay(double value, WindowLevel window)
    {
        if (value <= window.Lower)
            return OutputMin;
        if (value > window.Upper)
            return OutputMax;

        var normalized = (value - (window.Center - 0.5)) / (window.Width - 1) + 0.5;
        var scaled = normalized * (OutputMax - OutputMin) + OutputMin;
        return (byte)Math.Clamp(Math.Floor(scaled + 0.5), OutputMin, OutputMax);
    }

    /// <summary>
    /// Precomputes the output for every possible 16-bit input. Applying a window to an image
    /// is then one array lookup per pixel instead of floating-point math.
    /// </summary>
    public static byte[] BuildLookupTable(WindowLevel window)
    {
        var table = new byte[ushort.MaxValue + 1];
        for (var i = 0; i < table.Length; i++)
            table[i] = ToDisplay(short.MinValue + i, window);
        return table;
    }

    public static void Apply(ReadOnlySpan<short> pixels, Span<byte> output, WindowLevel window)
    {
        if (output.Length < pixels.Length)
            throw new ArgumentException("Output buffer is smaller than the input.", nameof(output));

        var table = BuildLookupTable(window);
        for (var i = 0; i < pixels.Length; i++)
            output[i] = table[pixels[i] - short.MinValue];
    }

    public static byte[] Apply(ReadOnlySpan<short> pixels, WindowLevel window)
    {
        var output = new byte[pixels.Length];
        Apply(pixels, output, window);
        return output;
    }
}
