using ImageWindowing;

namespace ImageWindowing.Tests;

public class WindowingTransformTests
{
    private static readonly WindowLevel Brain = Presets.Brain; // C=40, W=80 -> visible range 0..79 HU

    [Theory]
    [InlineData(-1000, 0)]   // air
    [InlineData(0, 0)]       // exactly on the lower bound is still black
    [InlineData(79, 255)]    // upper bound
    [InlineData(80, 255)]
    [InlineData(1000, 255)]  // bone
    public void ToDisplay_ClampsOutsideTheWindow(double hu, byte expected)
    {
        Assert.Equal(expected, WindowingTransform.ToDisplay(hu, Brain));
    }

    [Fact]
    public void ToDisplay_MapsCenterToMidGray()
    {
        Assert.InRange(WindowingTransform.ToDisplay(40, Brain), 127, 129);
    }

    [Fact]
    public void ToDisplay_IsMonotonicAcrossTheWindow()
    {
        byte previous = 0;
        for (var hu = -200; hu <= 300; hu++)
        {
            var current = WindowingTransform.ToDisplay(hu, Brain);
            Assert.True(current >= previous, $"Output decreased at {hu} HU.");
            previous = current;
        }
    }

    [Fact]
    public void WidthOfOne_BehavesAsAThreshold()
    {
        var threshold = new WindowLevel(100, 1);

        Assert.Equal(0, WindowingTransform.ToDisplay(99, threshold));
        Assert.Equal(255, WindowingTransform.ToDisplay(100, threshold));
    }

    [Theory]
    [MemberData(nameof(AllPresets))]
    public void LookupTable_MatchesDirectComputationForEveryInput(string name, WindowLevel window)
    {
        var table = WindowingTransform.BuildLookupTable(window);

        Assert.Equal(65536, table.Length);
        for (var value = short.MinValue; value < short.MaxValue; value++)
            Assert.True(table[value - short.MinValue] == WindowingTransform.ToDisplay(value, window), $"{name}: mismatch at {value}.");
    }

    [Fact]
    public void Apply_UsesTheWindowForEveryPixel()
    {
        short[] pixels = [-1000, 0, 40, 79, 1000];

        var output = WindowingTransform.Apply(pixels, Brain);

        Assert.Equal(pixels.Select(p => WindowingTransform.ToDisplay(p, Brain)), output);
    }

    [Fact]
    public void Apply_RejectsTooSmallOutputBuffer()
    {
        var pixels = new short[10];
        var output = new byte[9];

        Assert.Throws<ArgumentException>(() => WindowingTransform.Apply(pixels, output, Brain));
    }

    public static TheoryData<string, WindowLevel> AllPresets()
    {
        var data = new TheoryData<string, WindowLevel>();
        foreach (var (name, window) in Presets.All)
            data.Add(name, window);
        return data;
    }
}
