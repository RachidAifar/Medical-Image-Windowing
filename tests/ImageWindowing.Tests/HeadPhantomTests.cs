using ImageWindowing;

namespace ImageWindowing.Tests;

public class HeadPhantomTests
{
    [Theory]
    [InlineData(0.95, 0.95, -1000)] // outside the head: air
    [InlineData(0, 0.9, 1000)]      // skull rim: bone
    [InlineData(0, -0.3, 40)]       // brain tissue
    [InlineData(0.22, 0, 0)]        // right ventricle: fluid
    [InlineData(0, 0.35, 60)]       // dense structure
    public void ValueAt_ReturnsExpectedTissue(double x, double y, short expectedHu)
    {
        Assert.Equal(expectedHu, HeadPhantom.ValueAt(x, y));
    }

    [Fact]
    public void Generate_ProducesSquareImage()
    {
        var pixels = HeadPhantom.Generate(64);

        Assert.Equal(64 * 64, pixels.Length);
        Assert.Equal(HeadPhantom.Air, pixels[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Generate_RejectsTooSmallSize(int size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HeadPhantom.Generate(size));
    }
}
