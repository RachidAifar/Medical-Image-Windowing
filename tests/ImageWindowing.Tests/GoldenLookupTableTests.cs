using System.Text.Json;
using ImageWindowing;

namespace ImageWindowing.Tests;

/// <summary>
/// Regression tests: the lookup table for every preset must match the recorded hash in
/// testdata/lut-golden.json. The TypeScript widget checks the same file, so both
/// implementations are guaranteed to render identical images.
/// </summary>
public class GoldenLookupTableTests
{
    private static readonly Dictionary<string, GoldenEntry> Golden =
        JsonSerializer.Deserialize<Dictionary<string, GoldenEntry>>(File.ReadAllText("lut-golden.json"))!;

    [Fact]
    public void GoldenFile_CoversEveryPreset()
    {
        Assert.Equal(Presets.All.Keys.Order(), Golden.Keys.Order());
    }

    [Theory]
    [InlineData("brain")]
    [InlineData("soft-tissue")]
    [InlineData("lung")]
    [InlineData("bone")]
    public void LookupTable_MatchesGoldenHash(string preset)
    {
        var entry = Golden[preset];

        Assert.Equal(entry.sha256, LookupTableHash.Compute(new WindowLevel(entry.center, entry.width)));
    }

    private sealed record GoldenEntry(double center, double width, string sha256);
}
