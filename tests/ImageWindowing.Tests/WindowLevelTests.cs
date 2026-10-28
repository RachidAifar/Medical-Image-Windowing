using ImageWindowing;

namespace ImageWindowing.Tests;

public class WindowLevelTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(-10)]
    [InlineData(double.NaN)]
    public void Constructor_RejectsWidthBelowOne(double width)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WindowLevel(40, width));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsNonFiniteCenter(double center)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WindowLevel(center, 80));
    }

    [Fact]
    public void LowerAndUpper_FollowDicomDefinition()
    {
        var window = new WindowLevel(40, 80);

        Assert.Equal(0, window.Lower);   // 40 - 0.5 - 79/2
        Assert.Equal(79, window.Upper);  // 40 - 0.5 + 79/2
    }

    [Theory]
    [InlineData(-1000, 1000)]
    [InlineData(0, 255)]
    [InlineData(5, 6)]
    public void FromRange_MapsMinToBlackAndMaxToWhite(double min, double max)
    {
        var window = WindowLevel.FromRange(min, max);

        Assert.Equal(0, WindowingTransform.ToDisplay(min, window));
        Assert.Equal(255, WindowingTransform.ToDisplay(max, window));
    }

    [Fact]
    public void FromRange_WithEqualBounds_BecomesAThreshold()
    {
        var window = WindowLevel.FromRange(5, 5);

        Assert.Equal(1, window.Width);
        Assert.Equal(0, WindowingTransform.ToDisplay(5, window));
        Assert.Equal(255, WindowingTransform.ToDisplay(6, window));
    }

    [Fact]
    public void FromRange_RejectsInvertedRange()
    {
        Assert.Throws<ArgumentException>(() => WindowLevel.FromRange(10, -10));
    }
}
