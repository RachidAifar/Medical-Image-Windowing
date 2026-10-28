using System.Security.Cryptography;

namespace ImageWindowing;

/// <summary>
/// Fingerprint of a window's lookup table. The C# library and the TypeScript widget must
/// produce the same hash for the same window; the regression tests in both projects check this.
/// </summary>
public static class LookupTableHash
{
    public static string Compute(WindowLevel window) =>
        Convert.ToHexString(SHA256.HashData(WindowingTransform.BuildLookupTable(window))).ToLowerInvariant();
}
